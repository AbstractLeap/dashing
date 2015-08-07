namespace Dashing.Console.Weaving.Weavers {
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
            TypeDefinition typeDef,
            AssemblyDefinition assemblyDefinition,
            MapDefinition mapDefinition,
            Dictionary<string, List<MapDefinition>> assemblyMapDefinitions,
            Dictionary<string, AssemblyDefinition> assemblyDefinitions) {
            var boolTypeDef = typeDef.Module.Import(typeof(bool));
            foreach (
                var columnDef in
                    mapDefinition.ColumnDefinitions.Where(
                        c => c.Relationship == RelationshipType.ManyToOne || c.Relationship == RelationshipType.OneToOne)) {
                
                // remember the property may be defined on a parent class
                var propDef = this.GetProperty(typeDef, columnDef.Name);

                // add a field with DbType and DbName 
                TypeReference fkTypeReference;
                var fkPkType = columnDef.DbType.GetCLRType();
                if (fkPkType.IsValueType) {
                    fkTypeReference = typeDef.Module.Import(typeof(Nullable<>).MakeGenericType(fkPkType));
                }
                else {
                    fkTypeReference = typeDef.Module.Import(fkPkType);
                }

                var fkField = new FieldDefinition(columnDef.DbName, FieldAttributes.Public, fkTypeReference);
                if (propDef.DeclaringType.Fields.Any(f => f.Name == columnDef.DbName)) {
                    continue; // already done something here!
                }

                propDef.DeclaringType.Fields.Add(fkField);
                
                // override the set method to set to null
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Initobj, fkTypeReference));
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldflda, fkField));
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));

                // override the get method to access this field if null and create a new instance
                // TODO solve for non auto properties
                if (!propDef.GetMethod.Body.Variables.Any()) { // Release code is different to debug code!
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
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldfld, backingField));
                var skipToEnd = Instruction.Create(OpCodes.Ldc_I4_1);
                il.Insert(index++, Instruction.Create(OpCodes.Brtrue, skipToEnd));
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                if (fkPkType.IsValueType) {
                    il.Insert(index++, Instruction.Create(OpCodes.Ldflda, fkField));
                    il.Insert(index++,
                        Instruction.Create(
                            OpCodes.Call,
                            MakeGeneric(
                                typeDef.Module.Import(fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                                typeDef.Module.Import(fkPkType))));
                }
                else {
                    throw new NotImplementedException(); // need to do a null check
                }

                il.Insert(index++, Instruction.Create(OpCodes.Ldc_I4_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ceq));
                var nopInstr = Instruction.Create(OpCodes.Nop);
                il.Insert(index++, Instruction.Create(OpCodes.Br, nopInstr));
                il.Insert(index++, skipToEnd);
                il.Insert(index++, nopInstr);
                il.Insert(index++, Instruction.Create(OpCodes.Stloc_2));
                il.Insert(index++, Instruction.Create(OpCodes.Ldloc_2));
                il.Insert(index++, Instruction.Create(OpCodes.Brtrue, lastInstr));
                il.Insert(index++, Instruction.Create(OpCodes.Nop));
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                il.Insert(index++, Instruction.Create(OpCodes.Newobj, typeDef.Module.Import(propDef.PropertyType.Resolve().GetConstructors().First())));
                il.Insert(index++, Instruction.Create(OpCodes.Stloc_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldloc_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldarg_0));
                il.Insert(index++, Instruction.Create(OpCodes.Ldflda, fkField));
                il.Insert(index++,
                    Instruction.Create(
                        OpCodes.Call,
                        typeDef.Module.Import(
                            MakeGeneric(fkField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_Value"), typeDef.Module.Import(fkPkType)))));
                var fkMapDef = assemblyMapDefinitions.SelectMany(am => am.Value).First(m => m.TypeFullName == columnDef.TypeFullName);
                var assemblyDef = assemblyDefinitions.Single(ad => ad.Value.FullName == fkMapDef.AssemblyFullName).Value;
                var fkMapTypeRef = GetTypeDefFromFullName(columnDef.TypeFullName, assemblyDef);
                il.Insert(index++,
                    Instruction.Create(
                        OpCodes.Callvirt,
                        typeDef.Module.Import(this.GetProperty(fkMapTypeRef, fkMapDef.ColumnDefinitions.Single(cd => cd.IsPrimaryKey).Name).SetMethod)));
                il.Insert(index++, Instruction.Create(OpCodes.Ldloc_0));
                il.Insert(index, Instruction.Create(OpCodes.Stfld, backingField));
            }
        }
    }
}