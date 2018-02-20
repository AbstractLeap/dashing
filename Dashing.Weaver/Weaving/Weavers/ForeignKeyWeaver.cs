namespace Dashing.Weaver.Weaving.Weavers {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Extensions;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class ForeignKeyWeaver : BaseWeaver {
        public override void Weave(
            AssemblyDefinition assemblyDefinition,
            TypeDefinition typeDefinition,
            IEnumerable<ColumnDefinition> columnDefinitions) {
            var boolTypeDef = typeDefinition.Module.ImportReference(typeof(bool));
            foreach (var columnDef in columnDefinitions.Where(
                c => c.Relationship == RelationshipType.ManyToOne || c.Relationship == RelationshipType.OneToOne)) {
                // remember the property may be defined on a parent class
                var propDef = this.GetProperty(typeDefinition, columnDef.Name);

                // add a field with DbType and DbName 
                TypeReference fkTypeReference;
                var fkPkType = columnDef.DbType.GetCLRType();
                if (fkPkType.IsValueType()) {
                    fkTypeReference = typeDefinition.Module.ImportReference(typeof(Nullable<>).MakeGenericType(fkPkType));
                }
                else {
                    fkTypeReference = typeDefinition.Module.ImportReference(fkPkType);
                }

                var fkField = new FieldDefinition(columnDef.DbName, FieldAttributes.Public, fkTypeReference);
                if (propDef.DeclaringType.Fields.Any(f => f.Name == columnDef.DbName)) {
                    continue; // already done something here!
                }

                this.MakeNotDebuggerBrowsable(typeDefinition.Module, fkField);
                propDef.DeclaringType.Fields.Add(fkField);

                // override the set method to set to null
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Initobj, fkTypeReference));
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldflda, fkField));
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));

                // override the get method to access this field if null and create a new instance
                // TODO solve for non auto properties
                if (!propDef.GetMethod.Body.Variables.Any()) {
                    // Release code is different to debug code!
                    propDef.GetMethod.Body.Variables.Add(new VariableDefinition(propDef.PropertyType));
                }

                propDef.GetMethod.Body.Variables.Add(new VariableDefinition(propDef.PropertyType));
                propDef.GetMethod.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                propDef.GetMethod.Body.InitLocals = true;
                //propDef.GetMethod.Body.Instructions.Clear();

                var backingField = this.GetBackingField(propDef);
                var il = propDef.GetMethod.Body.Instructions;
                var lastInstr = il[0];
                var index = 0;

                // first bit does the null/hasValue checks on the backing fields
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldfld, backingField));

                il.Insert(index++, Instruction.Create(OpCodes.Brtrue, lastInstr));

                if (fkPkType.IsValueType()) {
                    il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                    il.Insert(index++, Instruction.Create(OpCodes.Ldflda, fkField));
                    il.Insert(
                        index++,
                        Instruction.Create(
                            OpCodes.Call,
                            MakeGeneric(
                                typeDefinition.Module.ImportReference(fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                                typeDefinition.Module.ImportReference(fkPkType))));
                }
                else {
                    il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                    il.Insert(index++, Instruction.Create(OpCodes.Ldfld, fkField));
                }

                il.Insert(index++, Instruction.Create(OpCodes.Brfalse, lastInstr));

                // if we have a pk but no ref we create a new instance with the primary key set
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                il.Insert(
                    index++,
                    Instruction.Create(
                        OpCodes.Newobj,
                        typeDefinition.Module.ImportReference(propDef.PropertyType.Resolve().GetConstructors().First())));
                il.Insert(index++, Instruction.Create(OpCodes.Stloc_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldloc_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));

                if (fkPkType.IsValueType()) {
                    il.Insert(index++, Instruction.Create(OpCodes.Ldflda, fkField));
                    il.Insert(
                        index++,
                        Instruction.Create(
                            OpCodes.Call,
                            typeDefinition.Module.ImportReference(
                                MakeGeneric(
                                    fkField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_Value"),
                                    typeDefinition.Module.ImportReference(fkPkType)))));

                    //var fkMapDef = assemblyMapDefinitions.SelectMany(am => am.Value).First(m => m.TypeFullName == columnDef.TypeFullName);
                    //var assemblyDef = assemblyDefinitions.Single(ad => ad.Value.FullName == fkMapDef.AssemblyFullName).Value;
                    //var fkMapTypeRef = GetTypeDefFromFullName(columnDef.TypeFullName, assemblyDef);
                    il.Insert(
                        index++,
                        Instruction.Create(
                            OpCodes.Callvirt,
                            typeDefinition.Module.ImportReference(
                                this.GetProperty(fkTypeReference.Resolve(), columnDef.RelatedTypePrimarykeyName).SetMethod)));
                }
                else {
                    il.Insert(index++, Instruction.Create(OpCodes.Ldfld, fkField));
                    //var fkMapDef = assemblyMapDefinitions.SelectMany(am => am.Value).First(m => m.TypeFullName == columnDef.TypeFullName);
                    //var assemblyDef = assemblyDefinitions.Single(ad => ad.Value.FullName == fkMapDef.AssemblyFullName).Value;
                    //var fkMapTypeRef = GetTypeDefFromFullName(columnDef.TypeFullName, assemblyDef);
                    il.Insert(
                        index++,
                        Instruction.Create(
                            OpCodes.Callvirt,
                            typeDefinition.Module.ImportReference(
                                this.GetProperty(fkTypeReference.Resolve(), columnDef.RelatedTypePrimarykeyName).SetMethod)));
                }

                il.Insert(index++, Instruction.Create(OpCodes.Ldloc_0));
                il.Insert(index, Instruction.Create(OpCodes.Stfld, backingField));
            }
        }
    }
}