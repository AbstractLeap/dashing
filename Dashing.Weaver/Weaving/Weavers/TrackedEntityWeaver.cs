﻿namespace Dashing.Weaver.Weaving.Weavers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Extensions;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    using FieldAttributes = Mono.Cecil.FieldAttributes;
    using MethodAttributes = Mono.Cecil.MethodAttributes;
    using ParameterAttributes = Mono.Cecil.ParameterAttributes;

    public class TrackedEntityWeaver : BaseWeaver {
        public override void Weave(
            AssemblyDefinition assemblyDefinition,
            TypeDefinition typeDefinition,
            IEnumerable<ColumnDefinition> columnDefinitions) {
            // this gets called with a typeDef set to something that's being mapped
            // but there's the possibility of each column belonging to a different parent class
            // so we'll find all of the class hierarchy and weave them individually
            var classHierarchy = this.GetClassHierarchy(typeDefinition);
            var totalInChain = classHierarchy.Count;

            // if there's only one class and that class is not extended elsewhere we'll use non-virtual methods
            var notInInheritance = totalInChain == 1
                                   && !assemblyDefinition.MainModule.Types.Any(
                                       t => t.IsClass && t.BaseType != null && t.BaseType.FullName == typeDefinition.FullName);

            while (classHierarchy.Count > 0) {
                this.ImplementITrackedEntityForTypeDefinition(classHierarchy.Pop(), columnDefinitions, notInInheritance);
            }
        }

        private void ImplementITrackedEntityForTypeDefinition(
            TypeDefinition typeDef,
            IEnumerable<ColumnDefinition> columnDefinitions,
            bool notInInheritance) {
            if (typeDef.Methods.Any(m => m.Name == "GetDirtyProperties")) {
                return; // type already woven
            }

            if (!typeDef.ImplementsInterface(typeof(ITrackedEntity))) {
                this.AddInterfaceToNonObjectAncestor(typeDef, typeof(ITrackedEntity));
            }

            // some common type definitions
            var boolTypeDef = typeDef.Module.ImportReference(typeof(bool));
            var voidTypeDef = typeDef.Module.ImportReference(typeof(void));
            var stringTypeDef = typeDef.Module.ImportReference(typeof(string));
            var listStringTypeDef = typeDef.Module.ImportReference(typeof(List<>)).MakeGenericInstanceType(stringTypeDef);
            var objectTypeDef = typeDef.Module.ImportReference(typeof(object));

            // some column names
            const string isTrackingName = "__isTracking";

            // add isTracking field if base class
            if (this.IsBaseClass(typeDef)) {
                var _isTrackingField = new FieldDefinition(isTrackingName, FieldAttributes.Family, boolTypeDef);
                this.MakeNotDebuggerBrowsable(typeDef.Module, _isTrackingField);
                typeDef.Fields.Add(_isTrackingField);
            }

            // fields for tracking state of properties on this class only
            var nonPkCols = columnDefinitions.Where(c => !c.IsPrimaryKey && c.Relationship != RelationshipType.OneToMany).ToList();
            foreach (var columnDefinition in nonPkCols) {
                if (this.HasPropertyInInheritanceChain(typeDef, columnDefinition.Name)) {
                    var propertyDefinition = this.GetProperty(typeDef, columnDefinition.Name);
                    if (propertyDefinition.DeclaringType.FullName == typeDef.FullName) {
                        var dirtyField = new FieldDefinition($"__{columnDefinition.Name}_IsDirty", FieldAttributes.Family, boolTypeDef);
                        this.MakeNotDebuggerBrowsable(typeDef.Module, dirtyField);
                        typeDef.Fields.Add(dirtyField);

                        // handle other maps, strings, valuetype, valuetype?
                        var oldValuePropType = propertyDefinition.PropertyType;
                        if (columnDefinition.Relationship == RelationshipType.None && propertyDefinition.PropertyType.IsValueType
                            && propertyDefinition.PropertyType.Name != "Nullable`1") {
                            oldValuePropType = typeDef.Module.ImportReference(typeof(Nullable<>)).MakeGenericInstanceType(oldValuePropType);
                            // use nullable value types
                        }

                        var oldValueField = new FieldDefinition($"__{columnDefinition.Name}_OldValue", FieldAttributes.Family, oldValuePropType);
                        this.MakeNotDebuggerBrowsable(typeDef.Module, oldValueField);
                        typeDef.Fields.Add(oldValueField);
                    }
                }
            }

            // insert the instructions in to the setter
            var isTrackingField = this.GetField(typeDef, isTrackingName);
            foreach (var columnDefinition in nonPkCols) {
                if (this.HasPropertyInInheritanceChain(typeDef, columnDefinition.Name)) {
                    var propertyDefinition = this.GetProperty(typeDef, columnDefinition.Name);
                    if (propertyDefinition.DeclaringType.FullName == typeDef.FullName) {
                        var backingField = this.GetBackingField(propertyDefinition);
                        var setter = propertyDefinition.SetMethod;
                        setter.Body.Variables.Add(new VariableDefinition(boolTypeDef)); // we need a local bool
                        setter.Body.InitLocals = true;
                        var setIl = setter.Body.Instructions;
                        var setIntructions = new List<Instruction>();                                               // -
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));                                   // 
                        setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));                               // 
                        setIntructions.Add(Instruction.Create(OpCodes.Ldfld, isTrackingField));          // 
                        setIntructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));                              // 
                        setIntructions.Add(Instruction.Create(OpCodes.Ceq));                                   // if (!this.isTracking) go to end instruction (although this is written as an IF statement in C# as if (this.isTracking)
                        setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));                               // 
                        setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));                               // 
                        var endNopInstr = Instruction.Create(OpCodes.Nop);                                          //
                        var endLdArgInstr = setIl.First();                                                //
                        setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endLdArgInstr));           // -
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));                                                                                              // -
                        setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));                                                                                          //
                        setIntructions.Add(                                                                                                                                    //
                            Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == $"__{columnDefinition.Name}_IsDirty")));      // if (__field_IsDirty) go to end (as we're already dirty)
                        setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));                                                                                          //
                        setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));                                                                                          //
                        setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endNopInstr));                                                                        // -
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));

                        if (propertyDefinition.PropertyType.IsValueType) {
                            var isEnum = propertyDefinition.PropertyType.Resolve().IsEnum;
                            if (isEnum) {
                                setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                                setIntructions.Add(Instruction.Create(OpCodes.Box, propertyDefinition.PropertyType));
                            }
                            else {
                                setIntructions.Add(Instruction.Create(OpCodes.Ldflda, backingField));
                            }

                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                            if (isEnum) {
                                setIntructions.Add(Instruction.Create(OpCodes.Box, propertyDefinition.PropertyType));
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Callvirt,
                                        typeDef.Module.ImportReference(
                                            objectTypeDef.Resolve()
                                                         .GetMethods()
                                                         .Single(
                                                             m => m.Name == "Equals" && m.Parameters.Count == 1
                                                                  && m.Parameters.First().ParameterType.Name.ToLowerInvariant()
                                                                  == "object"))));
                            }
                            else if (propertyDefinition.PropertyType.Name == "Nullable`1") {
                                setIntructions.Add(Instruction.Create(OpCodes.Box, backingField.FieldType));
                                setIntructions.Add(Instruction.Create(OpCodes.Constrained, backingField.FieldType));
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Callvirt,
                                        typeDef.Module.ImportReference(
                                            objectTypeDef.Resolve()
                                                         .GetMethods()
                                                         .Single(
                                                             m => m.Name == "Equals" && m.Parameters.Count == 1
                                                                  && m.Parameters.First().ParameterType.Name.ToLowerInvariant()
                                                                  == "object"))));
                            }
                            else {
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Call,
                                        typeDef.Module.ImportReference(
                                            propertyDefinition.PropertyType.Resolve()
                                                              .Methods
                                                              .Single(
                                                                  m => m.Name == "Equals" && m.Parameters.Count == 1
                                                                       && m.Parameters.First().ParameterType.Name.ToLowerInvariant()
                                                                       != "object"))));
                            }

                            setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endNopInstr));
                        }
                        else {
                            var fkPkType = columnDefinition.DbType.GetCLRType();
                            TypeReference fkTypeReference;
                            if (fkPkType.IsValueType()) {
                                fkTypeReference = typeDef.Module.ImportReference(typeof(Nullable<>).MakeGenericType(fkPkType));
                            }
                            else {
                                fkTypeReference = typeDef.Module.ImportReference(fkPkType);
                            }

                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            var hmmInstr = Instruction.Create(OpCodes.Ldc_I4_0);
                            var hmmInstr2 = Instruction.Create(OpCodes.Ldc_I4_1);

                            if (propertyDefinition.PropertyType.Name.ToLowerInvariant() == "string") {
                                var orInstr = Instruction.Create(OpCodes.Ldarg_0);
                                setIntructions.Add(Instruction.Create(OpCodes.Brtrue, orInstr));
                                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                                setIntructions.Add(Instruction.Create(OpCodes.Brtrue, hmmInstr));
                                setIntructions.Add(orInstr);
                            }
                            else {
                                var orInstr = Instruction.Create(OpCodes.Ldarg_1);
                                var orInstr2 = Instruction.Create(OpCodes.Ldarg_0);
                                setIntructions.Add(Instruction.Create(OpCodes.Brtrue, orInstr)); // k__BackingField != null
                                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                                setIntructions.Add(Instruction.Create(OpCodes.Brfalse, orInstr)); // value != null (k__BackingField == null

                                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                                if (fkPkType.IsValueType())
                                {
                                    // need to call HasValue
                                    setIntructions.Add(
                                        Instruction.Create(OpCodes.Ldflda, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                    setIntructions.Add(
                                        Instruction.Create(
                                            OpCodes.Call,
                                            MakeGeneric(
                                                typeDef.Module.ImportReference(
                                                    fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                                                typeDef.Module.ImportReference(fkPkType))));
                                }
                                else
                                {
                                    // check for null
                                    setIntructions.Add(
                                        Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                }

                                setIntructions.Add(Instruction.Create(OpCodes.Brfalse, hmmInstr));

                                // load the value of the Field
                                var pkPropDef = this.GetProperty(
                                    typeDef.Module.ImportReference(propertyDefinition.PropertyType.Resolve())
                                           .Resolve(),
                                    columnDefinition.RelatedTypePrimarykeyName);
                                if (fkPkType.IsValueType())
                                {
                                    setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    setIntructions.Add(
                                        Instruction.Create(OpCodes.Ldflda, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                    setIntructions.Add(
                                        Instruction.Create(
                                            OpCodes.Call,
                                            MakeGeneric(
                                                typeDef.Module.ImportReference(
                                                    fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_Value")),
                                                typeDef.Module.ImportReference(fkPkType))));
                                    if (string.Equals(pkPropDef.PropertyType.Name, "guid", StringComparison.InvariantCultureIgnoreCase)) {
                                        // we're calling an instance method for Equals so we have to store the value and use the address
                                        var guidPkVar = new VariableDefinition(pkPropDef.PropertyType);
                                        setter.Body.Variables.Add(guidPkVar);
                                        setIntructions.Add(Instruction.Create(OpCodes.Stloc, guidPkVar));
                                        setIntructions.Add(Instruction.Create(OpCodes.Ldloca, guidPkVar));
                                    }
                                }
                                else
                                {
                                    setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                                    setIntructions.Add(
                                        Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                }

                                // load the value of the primary key from the value

                                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                                setIntructions.Add(
                                    Instruction.Create(OpCodes.Callvirt, typeDef.Module.ImportReference(pkPropDef.GetMethod))); // value.PK

                                if (pkPropDef.PropertyType.IsPrimitive) {
                                    setIntructions.Add(Instruction.Create(OpCodes.Bne_Un, hmmInstr));
                                }
                                else if (string.Equals(pkPropDef.PropertyType.Name, "string", StringComparison.InvariantCultureIgnoreCase)) {
                                    setIntructions.Add(Instruction.Create(OpCodes.Call, typeDef.Module.ImportReference(pkPropDef.PropertyType.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.IsStatic && m.Parameters.Count == 2))));
                                    setIntructions.Add(Instruction.Create(OpCodes.Brfalse, hmmInstr));
                                } else if (string.Equals(pkPropDef.PropertyType.Name, "guid", StringComparison.InvariantCultureIgnoreCase)) {
                                    setIntructions.Add(Instruction.Create(OpCodes.Call, typeDef.Module.ImportReference(pkPropDef.PropertyType.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.IsPublic && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName != objectTypeDef.FullName)))); // we want the .Equals(Guid)
                                    setIntructions.Add(Instruction.Create(OpCodes.Brfalse, hmmInstr));
                                }
                                else {
                                    setIntructions.Add(Instruction.Create(OpCodes.Call, typeDef.Module.ImportReference(objectTypeDef.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.IsStatic && m.Parameters.Count == 2))));
                                    setIntructions.Add(Instruction.Create(OpCodes.Brfalse, hmmInstr));
                                }

                                //setIntructions.Add(
                                //    Instruction.Create(
                                //        OpCodes.Newobj,
                                //        typeDef.Module.ImportReference(propertyDefinition.PropertyType.Resolve().GetConstructors().First())))


                                setIntructions.Add(orInstr);
                                setIntructions.Add(Instruction.Create(OpCodes.Brtrue, orInstr2)); // value != null
                                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0)); 

                                if (fkPkType.IsValueType()) {
                                    // need to call HasValue
                                    setIntructions.Add(
                                        Instruction.Create(OpCodes.Ldflda, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                    setIntructions.Add(
                                        Instruction.Create(
                                            OpCodes.Call,
                                            MakeGeneric(
                                                typeDef.Module.ImportReference(
                                                    fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                                                typeDef.Module.ImportReference(fkPkType))));
                                    setIntructions.Add(Instruction.Create(OpCodes.Brtrue, hmmInstr)); // FieldId.HasValue
                                }
                                else {
                                    // check for null
                                    setIntructions.Add(
                                        Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                    setIntructions.Add(Instruction.Create(OpCodes.Brtrue, hmmInstr)); // FieldId != null
                                }

                                setIntructions.Add(orInstr2);
                            }

                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            setIntructions.Add(Instruction.Create(OpCodes.Brfalse, hmmInstr2)); // this.k__BackingField == null
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField)); 
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                            if (propertyDefinition.PropertyType.Name.ToLowerInvariant() == "string") {
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Callvirt,
                                        typeDef.Module.ImportReference(
                                            propertyDefinition.PropertyType.Resolve()
                                                              .GetMethods()
                                                              .Single(
                                                                  m => m.Name == "Equals" && m.Parameters.Count == 1
                                                                       && m.Parameters.First().ParameterType.Name.ToLowerInvariant()
                                                                       == "string")))); 
                            }
                            else {
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Callvirt,
                                        typeDef.Module.ImportReference(
                                            objectTypeDef.Resolve()
                                                         .GetMethods()
                                                         .Single(
                                                             m => m.Name == "Equals" && m.Parameters.Count == 1
                                                                  && m.Parameters.First().ParameterType.Name.ToLowerInvariant()
                                                                  == "object"))));
                            }

                            var nopInstr = Instruction.Create(OpCodes.Nop);
                            setIntructions.Add(Instruction.Create(OpCodes.Br, nopInstr)); // // this.k_BackingField (== | Equals) value
                            setIntructions.Add(hmmInstr2);
                            setIntructions.Add(nopInstr);
                            var nopInstr2 = Instruction.Create(OpCodes.Nop);
                            setIntructions.Add(Instruction.Create(OpCodes.Br, nopInstr2)); // 
                            setIntructions.Add(hmmInstr);
                            setIntructions.Add(nopInstr2);
                            setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endNopInstr)); // if line above equals jump to end, not dirty
                            setIntructions.Add(Instruction.Create(OpCodes.Nop));
                        }

                        // it's now dirty
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));

                        var topOfSetIsDirtyInstr = Instruction.Create(OpCodes.Ldarg_0);
                        if (columnDefinition.Relationship == RelationshipType.ManyToOne
                            || columnDefinition.Relationship == RelationshipType.OneToOne) {
                            // we need to check whether the foreign key backing field has a value
                            var setToBackingFieldInstr = Instruction.Create(OpCodes.Ldarg_0);
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            setIntructions.Add(Instruction.Create(OpCodes.Brtrue, setToBackingFieldInstr));
                            var fkPkType = columnDefinition.DbType.GetCLRType();
                            TypeReference fkTypeReference;
                            if (fkPkType.IsValueType()) {
                                fkTypeReference = typeDef.Module.ImportReference(typeof(Nullable<>).MakeGenericType(fkPkType));
                            }
                            else {
                                fkTypeReference = typeDef.Module.ImportReference(fkPkType);
                            }

                            var fkField = typeDef.Fields.Single(f => f.Name == columnDefinition.DbName);
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            if (fkPkType.IsValueType()) {
                                // need to call HasValue
                                setIntructions.Add(Instruction.Create(OpCodes.Ldflda, fkField));
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Call,
                                        MakeGeneric(
                                            typeDef.Module.ImportReference(
                                                fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                                            typeDef.Module.ImportReference(fkPkType))));
                                setIntructions.Add(Instruction.Create(OpCodes.Brfalse, setToBackingFieldInstr));
                            }
                            else {
                                // check for null
                                setIntructions.Add(Instruction.Create(OpCodes.Ldfld, fkField));
                                setIntructions.Add(Instruction.Create(OpCodes.Brfalse, setToBackingFieldInstr));
                            }

                            // need to add a variable to hold the new obj
                            var fkGeneratedVariableDef = new VariableDefinition(propertyDefinition.PropertyType);
                            propertyDefinition.SetMethod.Body.Variables.Add(fkGeneratedVariableDef);

                            // if we get here then we have an FK value but null in this backing field so we need to create a new instance of the FK and set that as the old value
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            setIntructions.Add(
                                Instruction.Create(
                                    OpCodes.Newobj,
                                    typeDef.Module.ImportReference(propertyDefinition.PropertyType.Resolve().GetConstructors().First())));
                            setIntructions.Add(Instruction.Create(OpCodes.Stloc, fkGeneratedVariableDef));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldloc, fkGeneratedVariableDef));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            if (fkPkType.IsValueType()) {
                                setIntructions.Add(Instruction.Create(OpCodes.Ldflda, typeDef.Fields.Single(f => f.Name == columnDefinition.DbName)));
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Call,
                                        typeDef.Module.ImportReference(
                                            MakeGeneric(
                                                fkField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_Value"),
                                                typeDef.Module.ImportReference(fkPkType)))));
                                //var fkMapDef = assemblyMapDefinitions.SelectMany(am => am.Value).First(m => m.TypeFullName == columnDefinition.TypeFullName);
                                //var assemblyDef = assemblyDefinitions.Single(ad => ad.Value.FullName == fkMapDef.AssemblyFullName).Value;
                                //var fkMapTypeRef = GetTypeDefFromFullName(columnDefinition.TypeFullName, assemblyDef);
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Callvirt,
                                        typeDef.Module.ImportReference(
                                            this.GetProperty(
                                                    propertyDefinition.PropertyType.Resolve(),
                                                    columnDefinition.RelatedTypePrimarykeyName)
                                                .SetMethod)));
                            }
                            else {
                                setIntructions.Add(Instruction.Create(OpCodes.Ldfld, fkField));
                                //var fkMapDef = assemblyMapDefinitions.SelectMany(am => am.Value).First(m => m.TypeFullName == columnDefinition.TypeFullName);
                                //var assemblyDef = assemblyDefinitions.Single(ad => ad.Value.FullName == fkMapDef.AssemblyFullName).Value;
                                //var fkMapTypeRef = GetTypeDefFromFullName(columnDefinition.TypeFullName, assemblyDef);
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Callvirt,
                                        typeDef.Module.ImportReference(
                                            this.GetProperty(
                                                    propertyDefinition.PropertyType.Resolve(),
                                                    columnDefinition.RelatedTypePrimarykeyName)
                                                .SetMethod)));
                            }

                            setIntructions.Add(Instruction.Create(OpCodes.Ldloc, fkGeneratedVariableDef));
                            setIntructions.Add(
                                Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == $"__{columnDefinition.Name}_OldValue")));
                            setIntructions.Add(Instruction.Create(OpCodes.Br, topOfSetIsDirtyInstr));

                            // set using backing field
                            setIntructions.Add(setToBackingFieldInstr);
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            setIntructions.Add(
                                Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == $"__{columnDefinition.Name}_OldValue")));
                        }
                        else {
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            if (columnDefinition.Relationship == RelationshipType.None && propertyDefinition.PropertyType.IsValueType
                                && propertyDefinition.PropertyType.Name != "Nullable`1") {
                                setIntructions.Add(
                                    Instruction.Create(
                                        OpCodes.Newobj,
                                        MakeGeneric(
                                            typeDef.Module.ImportReference(
                                                typeDef.Fields.Single(f => f.Name == $"__{columnDefinition.Name}_OldValue")
                                                       .FieldType.Resolve()
                                                       .GetConstructors()
                                                       .First()),
                                            propertyDefinition.PropertyType)));
                            }

                            setIntructions.Add(
                                Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == $"__{columnDefinition.Name}_OldValue")));
                        }

                        setIntructions.Add(topOfSetIsDirtyInstr);
                        setIntructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                        setIntructions.Add(
                            Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == $"__{columnDefinition.Name}_IsDirty")));
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));
                        setIntructions.Add(endNopInstr);
                        setIntructions.Reverse();
                        foreach (var instruction in setIntructions) {
                            setIl.Insert(0, instruction);
                        }
                    }
                }
            }

            // implement the ITrackedEntity methods
            // EnableTracking
            if (this.IsBaseClass(typeDef)) {
                var enableTracking = new MethodDefinition(
                    "EnableTracking",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual
                    | MethodAttributes.Final,
                    voidTypeDef);
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == isTrackingName)));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                typeDef.Methods.Add(enableTracking);
            }

            // DisableTracking
            var disableTrackingMethodAttrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (notInInheritance) {
                disableTrackingMethodAttrs = disableTrackingMethodAttrs | MethodAttributes.NewSlot | MethodAttributes.Final;
            }
            var disableTracking = new MethodDefinition("DisableTracking", disableTrackingMethodAttrs, voidTypeDef);
            var disableInstructions = disableTracking.Body.Instructions;
            disableInstructions.Add(Instruction.Create(OpCodes.Nop));
            disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            disableInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            disableInstructions.Add(Instruction.Create(OpCodes.Stfld, isTrackingField));
            foreach (var col in nonPkCols) {
                if (this.HasPropertyInInheritanceChain(typeDef, col.Name)) {
                    var propDef = this.GetProperty(typeDef, col.Name);

                    // reset isdirty
                    disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    disableInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                    disableInstructions.Add(Instruction.Create(OpCodes.Stfld, this.GetField(typeDef, $"__{col.Name}_IsDirty")));

                    // reset oldvalue
                    disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    var oldValueField = this.GetField(typeDef, $"__{col.Name}_OldValue");
                    if (propDef.PropertyType.IsValueType) {
                        disableInstructions.Add(Instruction.Create(OpCodes.Ldflda, oldValueField));
                        disableInstructions.Add(Instruction.Create(OpCodes.Initobj, oldValueField.FieldType));
                    }
                    else {
                        disableInstructions.Add(Instruction.Create(OpCodes.Ldnull));
                        disableInstructions.Add(Instruction.Create(OpCodes.Stfld, oldValueField));
                    }
                }
            }

            disableInstructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(disableTracking);

            // IsTrackingEnabled
            if (this.IsBaseClass(typeDef)) {
                var isTrackingEnabled = new MethodDefinition(
                    "IsTrackingEnabled",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual
                    | MethodAttributes.Final,
                    boolTypeDef);
                isTrackingEnabled.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                isTrackingEnabled.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                isTrackingEnabled.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == isTrackingName)));
                isTrackingEnabled.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
                var loadInstr = Instruction.Create(OpCodes.Ldloc_0);
                isTrackingEnabled.Body.Instructions.Add(Instruction.Create(OpCodes.Br, loadInstr));
                isTrackingEnabled.Body.Instructions.Add(loadInstr);
                isTrackingEnabled.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                isTrackingEnabled.Body.InitLocals = true;
                isTrackingEnabled.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                typeDef.Methods.Add(isTrackingEnabled);
            }

            // GetDirtyProperties
            var getDirtyPropertiesMethodAttrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (notInInheritance) {
                getDirtyPropertiesMethodAttrs = getDirtyPropertiesMethodAttrs | MethodAttributes.NewSlot | MethodAttributes.Final;
            }
            var getDirtyProperties = new MethodDefinition(
                "GetDirtyProperties",
                getDirtyPropertiesMethodAttrs,
                typeDef.Module.ImportReference(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef));
            getDirtyProperties.Body.Variables.Add(new VariableDefinition(listStringTypeDef));
            getDirtyProperties.Body.Variables.Add(
                new VariableDefinition(
                    typeDef.Module.ImportReference(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef)));
            getDirtyProperties.Body.Variables.Add(new VariableDefinition(boolTypeDef));
            getDirtyProperties.Body.InitLocals = true;
            var instructions = getDirtyProperties.Body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            var listStringContruictor = MakeGeneric(
                typeDef.Module.ImportReference(
                    listStringTypeDef.Resolve().GetConstructors().First(c => !c.HasParameters && !c.IsStatic && c.IsPublic)),
                stringTypeDef);
            instructions.Add(Instruction.Create(OpCodes.Newobj, listStringContruictor));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));

            var breakToInstruction = Instruction.Create(nonPkCols.Count == 1 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            var addMethod = typeDef.Module.ImportReference(listStringTypeDef.Resolve().Methods.Single(m => m.Name == "Add"));
            addMethod = MakeGeneric(addMethod, stringTypeDef);
            var visibleCols = nonPkCols.Where(c => this.HasPropertyInInheritanceChain(typeDef, c.Name)).ToList();
            for (var i = 0; i < visibleCols.Count; i++) {
                if (i == 0) {
                    instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                }

                instructions.Add(Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, $"__{visibleCols.ElementAt(i).Name}_IsDirty")));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Stloc_2));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_2));
                instructions.Add(Instruction.Create(OpCodes.Brtrue, breakToInstruction));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, visibleCols.ElementAt(i).Name));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, addMethod));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(breakToInstruction);
                breakToInstruction = Instruction.Create(i == visibleCols.Count - 2 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            }

            instructions.Add(Instruction.Create(OpCodes.Stloc_1));
            var retInstr = Instruction.Create(OpCodes.Ldloc_1);
            instructions.Add(Instruction.Create(OpCodes.Br, retInstr));
            instructions.Add(retInstr);
            instructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getDirtyProperties);

            // GetOldValue
            var getOldValueMethodAttrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (notInInheritance) {
                getOldValueMethodAttrs = getOldValueMethodAttrs | MethodAttributes.NewSlot | MethodAttributes.Final;
            }

            var getOldValue = new MethodDefinition("GetOldValue", getOldValueMethodAttrs, objectTypeDef);
            getOldValue.Parameters.Add(new ParameterDefinition("propertyName", ParameterAttributes.None, stringTypeDef));
            getOldValue.Body.Variables.Add(new VariableDefinition(objectTypeDef));
            getOldValue.Body.Variables.Add(new VariableDefinition(stringTypeDef));
            getOldValue.Body.Variables.Add(new VariableDefinition(boolTypeDef));
            getOldValue.Body.InitLocals = true;
            var getBodyInstructions = getOldValue.Body.Instructions;
            getBodyInstructions.Add(Instruction.Create(OpCodes.Nop));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Stloc_1));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Ldloc_1));

            var throwExceptionTarget = Instruction.Create(OpCodes.Ldstr, "propertyName");
            var returnTarget = Instruction.Create(OpCodes.Ldloc_0);
            getBodyInstructions.Add(Instruction.Create(OpCodes.Brfalse, throwExceptionTarget));

            var switchInstructions = new List<Instruction>();
            var opEqualityRef = typeDef.Module.ImportReference(typeof(string).GetMethods().Single(m => m.Name == "op_Equality"));
            for (var i = 0; i < visibleCols.Count; i++) {
                // generate the switch bit
                getBodyInstructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                getBodyInstructions.Add(Instruction.Create(OpCodes.Ldstr, visibleCols.ElementAt(i).Name));
                getBodyInstructions.Add(Instruction.Create(OpCodes.Call, opEqualityRef));

                // generate the if bit
                var targetInstr = Instruction.Create(OpCodes.Ldarg_0);
                getBodyInstructions.Add(Instruction.Create(OpCodes.Brtrue, targetInstr));
                switchInstructions.Add(targetInstr);
                switchInstructions.Add(Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, $"__{visibleCols.ElementAt(i).Name}_IsDirty")));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                switchInstructions.Add(Instruction.Create(OpCodes.Ceq));
                switchInstructions.Add(Instruction.Create(OpCodes.Stloc_2));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldloc_2));

                // generate the return bit
                var breakInstruction = Instruction.Create(OpCodes.Br, throwExceptionTarget);
                switchInstructions.Add(Instruction.Create(OpCodes.Brtrue, breakInstruction));
                switchInstructions.Add(Instruction.Create(OpCodes.Nop));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, $"__{visibleCols.ElementAt(i).Name}_OldValue")));
                if (this.GetProperty(typeDef, visibleCols.ElementAt(i).Name).PropertyType.IsValueType) {
                    switchInstructions.Add(
                        Instruction.Create(OpCodes.Box, this.GetField(typeDef, $"__{visibleCols.ElementAt(i).Name}_OldValue").FieldType));
                }

                switchInstructions.Add(Instruction.Create(OpCodes.Stloc_0));
                switchInstructions.Add(Instruction.Create(OpCodes.Br, returnTarget));
                switchInstructions.Add(breakInstruction);
            }

            // add a br
            getBodyInstructions.Add(Instruction.Create(OpCodes.Br, throwExceptionTarget));

            // run them
            foreach (var instruction in switchInstructions) {
                getBodyInstructions.Add(instruction);
            }

            // handle the exception
            getBodyInstructions.Add(Instruction.Create(OpCodes.Nop));
            getBodyInstructions.Add(throwExceptionTarget);
            getBodyInstructions.Add(
                Instruction.Create(OpCodes.Ldstr, "Either the property doesn't exist or it's not dirty. Consult GetDirtyProperties first"));
            getBodyInstructions.Add(
                Instruction.Create(
                    OpCodes.Newobj,
                    typeDef.Module.ImportReference(
                        typeof(ArgumentOutOfRangeException)
                            .GetConstructors()
                            .First(c => c.GetParameters().All(p => p.ParameterType == typeof(string)) && c.GetParameters().Length == 2))));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Throw));
            getBodyInstructions.Add(returnTarget);
            getBodyInstructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getOldValue);
        }
    }
}