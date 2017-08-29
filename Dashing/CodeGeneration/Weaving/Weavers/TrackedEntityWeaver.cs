namespace Dashing.CodeGeneration.Weaving.Weavers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

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
                var _isTrackingField = new FieldDefinition(isTrackingName, Mono.Cecil.FieldAttributes.Family, boolTypeDef);
                this.MakeNotDebuggerBrowsable(typeDef.Module, _isTrackingField);
                typeDef.Fields.Add(_isTrackingField);
            }

            // fields for tracking state of properties on this class only
            var nonPkCols = columnDefinitions.Where(c => !c.IsPrimaryKey && c.Relationship != RelationshipType.OneToMany).ToList();
            foreach (var columnDefinition in nonPkCols) {
                if (this.HasPropertyInInheritanceChain(typeDef, columnDefinition.Name)) {
                    var propertyDefinition = this.GetProperty(typeDef, columnDefinition.Name);
                    if (propertyDefinition.DeclaringType.FullName == typeDef.FullName) {
                        var dirtyField = new FieldDefinition(
                            string.Format("__{0}_IsDirty", columnDefinition.Name),
                            Mono.Cecil.FieldAttributes.Family,
                            boolTypeDef);
                        this.MakeNotDebuggerBrowsable(typeDef.Module, dirtyField);
                        typeDef.Fields.Add(dirtyField);

                        // handle other maps, strings, valuetype, valuetype?
                        var oldValuePropType = propertyDefinition.PropertyType;
                        if (columnDefinition.Relationship == RelationshipType.None && propertyDefinition.PropertyType.IsValueType
                            && propertyDefinition.PropertyType.Name != "Nullable`1") {
                            oldValuePropType = typeDef.Module.ImportReference(typeof(Nullable<>)).MakeGenericInstanceType(oldValuePropType);
                            // use nullable value types
                        }

                        var oldValueField = new FieldDefinition(
                            string.Format("__{0}_OldValue", columnDefinition.Name),
                            Mono.Cecil.FieldAttributes.Family,
                            oldValuePropType);
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
                        var setIntructions = new List<Instruction>();
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldfld, isTrackingField));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                        setIntructions.Add(Instruction.Create(OpCodes.Ceq));
                        setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                        var endNopInstr = Instruction.Create(OpCodes.Nop);
                        var endLdArgInstr = setIl.First();
                        setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endLdArgInstr));
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        setIntructions.Add(
                            Instruction.Create(
                                OpCodes.Ldfld,
                                typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsDirty", columnDefinition.Name))));
                        setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                        setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endNopInstr));
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
                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            var orInstr = Instruction.Create(OpCodes.Ldarg_0);
                            var hmmInstr = Instruction.Create(OpCodes.Ldc_I4_0);
                            var hmmInstr2 = Instruction.Create(OpCodes.Ldc_I4_1);
                            setIntructions.Add(Instruction.Create(OpCodes.Brtrue, orInstr));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                            setIntructions.Add(Instruction.Create(OpCodes.Brtrue, hmmInstr));
                            setIntructions.Add(orInstr);
                            setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                            setIntructions.Add(Instruction.Create(OpCodes.Brfalse, hmmInstr2));
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
                            setIntructions.Add(Instruction.Create(OpCodes.Br, nopInstr));
                            setIntructions.Add(hmmInstr2);
                            setIntructions.Add(nopInstr);
                            var nopInstr2 = Instruction.Create(OpCodes.Nop);
                            setIntructions.Add(Instruction.Create(OpCodes.Br, nopInstr2));
                            setIntructions.Add(hmmInstr);
                            setIntructions.Add(nopInstr2);
                            setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                            setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endNopInstr));
                            setIntructions.Add(Instruction.Create(OpCodes.Nop));
                        }

                        // it's now dirty
                        setIntructions.Add(Instruction.Create(OpCodes.Nop));
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
                                            typeDef.Fields.Single(f => f.Name == string.Format("__{0}_OldValue", columnDefinition.Name))
                                                   .FieldType.Resolve()
                                                   .GetConstructors()
                                                   .First()),
                                        propertyDefinition.PropertyType)));
                        }

                        setIntructions.Add(
                            Instruction.Create(
                                OpCodes.Stfld,
                                typeDef.Fields.Single(f => f.Name == string.Format("__{0}_OldValue", columnDefinition.Name))));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                        setIntructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                        setIntructions.Add(
                            Instruction.Create(
                                OpCodes.Stfld,
                                typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsDirty", columnDefinition.Name))));
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
                    Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.NewSlot | Mono.Cecil.MethodAttributes.Virtual
                    | Mono.Cecil.MethodAttributes.Final,
                    voidTypeDef);
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == isTrackingName)));
                enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                typeDef.Methods.Add(enableTracking);
            }

            // DisableTracking
            var disableTrackingMethodAttrs = Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual;
            if (notInInheritance) {
                disableTrackingMethodAttrs = disableTrackingMethodAttrs | Mono.Cecil.MethodAttributes.NewSlot | Mono.Cecil.MethodAttributes.Final;
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
                    disableInstructions.Add(Instruction.Create(OpCodes.Stfld, this.GetField(typeDef, string.Format("__{0}_IsDirty", col.Name))));

                    // reset oldvalue
                    disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    var oldValueField = this.GetField(typeDef, string.Format("__{0}_OldValue", col.Name));
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
                    Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.NewSlot | Mono.Cecil.MethodAttributes.Virtual
                    | Mono.Cecil.MethodAttributes.Final,
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
            var getDirtyPropertiesMethodAttrs = Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual;
            if (notInInheritance) {
                getDirtyPropertiesMethodAttrs = getDirtyPropertiesMethodAttrs | Mono.Cecil.MethodAttributes.NewSlot | Mono.Cecil.MethodAttributes.Final;
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

                instructions.Add(
                    Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, string.Format("__{0}_IsDirty", visibleCols.ElementAt(i).Name))));
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
            var getOldValueMethodAttrs = Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual;
            if (notInInheritance) {
                getOldValueMethodAttrs = getOldValueMethodAttrs | Mono.Cecil.MethodAttributes.NewSlot | Mono.Cecil.MethodAttributes.Final;
            }

            var getOldValue = new MethodDefinition("GetOldValue", getOldValueMethodAttrs, objectTypeDef);
            getOldValue.Parameters.Add(new ParameterDefinition("propertyName", Mono.Cecil.ParameterAttributes.None, stringTypeDef));
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
                switchInstructions.Add(
                    Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, String.Format("__{0}_IsDirty", visibleCols.ElementAt(i).Name))));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                switchInstructions.Add(Instruction.Create(OpCodes.Ceq));
                switchInstructions.Add(Instruction.Create(OpCodes.Stloc_2));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldloc_2));

                // generate the return bit
                var breakInstruction = Instruction.Create(OpCodes.Br, throwExceptionTarget);
                switchInstructions.Add(Instruction.Create(OpCodes.Brtrue, breakInstruction));
                switchInstructions.Add(Instruction.Create(OpCodes.Nop));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                switchInstructions.Add(
                    Instruction.Create(OpCodes.Ldfld, this.GetField(typeDef, String.Format("__{0}_OldValue", visibleCols.ElementAt(i).Name))));
                if (this.GetProperty(typeDef, visibleCols.ElementAt(i).Name).PropertyType.IsValueType) {
                    switchInstructions.Add(
                        Instruction.Create(
                            OpCodes.Box,
                            this.GetField(typeDef, String.Format("__{0}_OldValue", visibleCols.ElementAt(i).Name)).FieldType));
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
                            .First(c => c.GetParameters().All(p => p.ParameterType == typeof(string)) && c.GetParameters().Count() == 2))));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Throw));
            getBodyInstructions.Add(returnTarget);
            getBodyInstructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getOldValue);
        }
    }
}