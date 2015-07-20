namespace Dashing.CodeGeneration.Weaving.Weavers {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class EqualityWeaver : BaseWeaver {
        public override void Weave(
            TypeDefinition typeDef,
            AssemblyDefinition assemblyDefinition,
            MapDefinition mapDefinition,
            Dictionary<string, List<MapDefinition>> assemblyMapDefinitions,
            Dictionary<string, AssemblyDefinition> assemblyDefinitions) {
            var intTypeDef = typeDef.Module.Import(typeof(int));
            var boolTypeDef = typeDef.Module.Import(typeof(bool));
            var objectTypeDef = typeDef.Module.Import(typeof(object));
            var pkColDef = this.GetProperty(typeDef, mapDefinition.ColumnDefinitions.Single(d => d.IsPrimaryKey).Name);

            if (!this.DoesNotUseObjectMethod(typeDef, "GetHashCode")) {
                // override gethashcode
                var hashCodeBackingField = new FieldDefinition(
                    "__hashcode",
                    FieldAttributes.Private,
                    typeDef.Module.Import(typeof(Nullable<>).MakeGenericType(typeof(int))));
                typeDef.Fields.Add(hashCodeBackingField);

                var method = new MethodDefinition(
                    "GetHashCode",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    intTypeDef);
                method.Body.Variables.Add(new VariableDefinition(intTypeDef));
                method.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                method.Body.InitLocals = true;

                var il = method.Body.Instructions;
                var getHasValueMethodRef =
                    MakeGeneric(
                        typeDef.Module.Import(hashCodeBackingField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                        typeDef.Module.Import(typeof(int)));
                var getValueMethodRef =
                    MakeGeneric(
                        typeDef.Module.Import(hashCodeBackingField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_Value")),
                        typeDef.Module.Import(typeof(int)));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Call, getHasValueMethodRef));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                il.Add(Instruction.Create(OpCodes.Ldloc_1));
                var getIdInstr = Instruction.Create(OpCodes.Ldarg_0);
                il.Add(Instruction.Create(OpCodes.Brtrue, getIdInstr));
                il.Add(Instruction.Create(OpCodes.Nop));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Call, getValueMethodRef));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                var endInstr = Instruction.Create(OpCodes.Ldloc_0);
                il.Add(Instruction.Create(OpCodes.Br, endInstr));
                il.Add(getIdInstr);
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                il.Add(Instruction.Create(OpCodes.Ldloc_1));
                var getIdInstr2 = Instruction.Create(OpCodes.Nop);
                il.Add(Instruction.Create(OpCodes.Brtrue, getIdInstr2));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Call, typeDef.Module.Import(typeof(object).GetMethods().Single(m => m.Name == "GetHashCode"))));
                il.Add(
                    Instruction.Create(
                        OpCodes.Newobj,
                        MakeGeneric(
                            typeDef.Module.Import(hashCodeBackingField.FieldType.Resolve().GetConstructors().First()),
                            typeDef.Module.Import(typeof(int)))));
                il.Add(Instruction.Create(OpCodes.Stfld, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Call, getValueMethodRef));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                il.Add(Instruction.Create(OpCodes.Br, endInstr));
                il.Add(getIdInstr2);
                il.Add(Instruction.Create(OpCodes.Ldc_I4, 17 * 29));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Add));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                il.Add(Instruction.Create(OpCodes.Br, endInstr));
                il.Add(endInstr);
                il.Add(Instruction.Create(OpCodes.Ret));
                typeDef.Methods.Add(method);
            }

            if (!this.DoesNotUseObjectMethod(typeDef, "Equals")) {
                // override equals
                var equals = new MethodDefinition(
                    "Equals",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    boolTypeDef);
                equals.Parameters.Add(new ParameterDefinition(objectTypeDef));
                equals.Body.InitLocals = true;
                equals.Body.Variables.Add(new VariableDefinition(typeDef));
                equals.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                equals.Body.Variables.Add(new VariableDefinition(boolTypeDef));

                var il = equals.Body.Instructions;
                il.Add(Instruction.Create(OpCodes.Ldarg_1));
                il.Add(Instruction.Create(OpCodes.Ldnull));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Stloc_2));
                il.Add(Instruction.Create(OpCodes.Ldloc_2));
                var isInstInstr = Instruction.Create(OpCodes.Ldarg_1);
                il.Add(Instruction.Create(OpCodes.Brtrue, isInstInstr));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                var ret = Instruction.Create(OpCodes.Ldloc_1);
                il.Add(Instruction.Create(OpCodes.Br, ret));
                il.Add(isInstInstr);
                il.Add(Instruction.Create(OpCodes.Isinst, typeDef));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                il.Add(Instruction.Create(OpCodes.Ldloc_0));
                il.Add(Instruction.Create(OpCodes.Ldnull));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Stloc_2));
                il.Add(Instruction.Create(OpCodes.Ldloc_2));
                var getIdInstr = Instruction.Create(OpCodes.Ldarg_0);
                il.Add(Instruction.Create(OpCodes.Brtrue, getIdInstr));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                il.Add(Instruction.Create(OpCodes.Br, ret));
                il.Add(getIdInstr);
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Stloc_2));
                il.Add(Instruction.Create(OpCodes.Ldloc_2));
                var getIdInstr2 = Instruction.Create(OpCodes.Ldarg_0);
                il.Add(Instruction.Create(OpCodes.Brtrue, getIdInstr2));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                il.Add(Instruction.Create(OpCodes.Br, ret));
                il.Add(getIdInstr2);
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Ldloc_0));
                il.Add(Instruction.Create(OpCodes.Callvirt, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Ceq));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                il.Add(Instruction.Create(OpCodes.Br, ret));
                il.Add(ret);
                il.Add(Instruction.Create(OpCodes.Ret));
                typeDef.Methods.Add(equals);
            }
        }
    }
}