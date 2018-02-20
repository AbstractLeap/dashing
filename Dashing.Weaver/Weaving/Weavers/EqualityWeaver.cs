namespace Dashing.Weaver.Weaving.Weavers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Rocks;

    public class EqualityWeaver : BaseWeaver {
        public override void Weave(
            AssemblyDefinition assemblyDefinition,
            TypeDefinition typeDefinition,
            IEnumerable<ColumnDefinition> columnDefinitions) {
            var intTypeDef = typeDefinition.Module.ImportReference(typeof(int));
            var guidTypeDef = typeDefinition.Module.ImportReference(typeof(Guid));
            var boolTypeDef = typeDefinition.Module.ImportReference(typeof(bool));
            var objectTypeDef = typeDefinition.Module.ImportReference(typeof(object));
            var pkColDef = this.GetProperty(typeDefinition, columnDefinitions.Single(d => d.IsPrimaryKey).Name);
            var isGuidPk = pkColDef.PropertyType.Name == "Guid";
            var isStringPk = !isGuidPk && pkColDef.PropertyType.Name.Equals("string", StringComparison.OrdinalIgnoreCase);

            if (!this.DoesNotUseObjectMethod(typeDefinition, "GetHashCode")) {
                // override gethashcode
                var hashCodeBackingField = new FieldDefinition(
                    "__hashcode",
                    Mono.Cecil.FieldAttributes.Private,
                    typeDefinition.Module.ImportReference(typeof(Nullable<>).MakeGenericType(typeof(int))));
                typeDefinition.Fields.Add(hashCodeBackingField);

                var method = new MethodDefinition(
                    "GetHashCode",
                    Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual,
                    intTypeDef);

                var variableType = isGuidPk ? guidTypeDef : intTypeDef;
                method.Body.Variables.Add(new VariableDefinition(intTypeDef));
                var var1 = new VariableDefinition(variableType);
                var var2 = new VariableDefinition(variableType);
                if (!isStringPk) {
                    method.Body.Variables.Add(var1);
                }

                if (isGuidPk) {
                    method.Body.Variables.Add(var2);
                }

                method.Body.InitLocals = true;

                var il = method.Body.Instructions;
                var getHasValueMethodRef = MakeGeneric(
                    typeDefinition.Module.ImportReference(
                        hashCodeBackingField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")),
                    typeDefinition.Module.ImportReference(typeof(int)));
                var getValueMethodRef = MakeGeneric(
                    typeDefinition.Module.ImportReference(hashCodeBackingField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_Value")),
                    typeDefinition.Module.ImportReference(typeof(int)));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Call, getHasValueMethodRef));
                var noValueOnBackingInstr = Instruction.Create(OpCodes.Ldarg_0);
                il.Add(Instruction.Create(OpCodes.Brfalse, noValueOnBackingInstr));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Call, getValueMethodRef));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                var endInstr = Instruction.Create(OpCodes.Ldloc_0);
                il.Add(Instruction.Create(OpCodes.Br, endInstr));
                il.Add(noValueOnBackingInstr);
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                var useIdInstr = Instruction.Create(OpCodes.Ldarg_0);
                if (isGuidPk) {
                    il.Add(Instruction.Create(OpCodes.Ldloca_S, var1));
                    il.Add(Instruction.Create(OpCodes.Initobj, guidTypeDef));
                    il.Add(Instruction.Create(OpCodes.Ldloc_1));
                    il.Add(
                        Instruction.Create(
                            OpCodes.Call,
                            typeDefinition.Module.ImportReference(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Equality"))));
                    il.Add(Instruction.Create(OpCodes.Brfalse_S, useIdInstr));
                }
                else {
                    il.Add(Instruction.Create(OpCodes.Brtrue_S, useIdInstr));
                }

                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(
                    Instruction.Create(
                        OpCodes.Call,
                        typeDefinition.Module.ImportReference(typeof(object).GetMethods().Single(m => m.Name == "GetHashCode"))));
                il.Add(
                    Instruction.Create(
                        OpCodes.Newobj,
                        MakeGeneric(
                            typeDefinition.Module.ImportReference(hashCodeBackingField.FieldType.Resolve().GetConstructors().First()),
                            typeDefinition.Module.ImportReference(typeof(int)))));
                il.Add(Instruction.Create(OpCodes.Stfld, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, hashCodeBackingField));
                il.Add(Instruction.Create(OpCodes.Call, getValueMethodRef));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                il.Add(Instruction.Create(OpCodes.Br_S, endInstr));
                il.Add(useIdInstr);
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                if (isGuidPk) {
                    il.Add(Instruction.Create(OpCodes.Stloc_2));
                    il.Add(Instruction.Create(OpCodes.Ldloca_S, var2));
                    il.Add(Instruction.Create(OpCodes.Constrained, guidTypeDef));
                    il.Add(
                        Instruction.Create(
                            OpCodes.Callvirt,
                            typeDefinition.Module.ImportReference(typeof(object).GetMethods().Single(m => m.Name == "GetHashCode"))));
                }
                else if (isStringPk) {
                    il.Add(
                        Instruction.Create(
                            OpCodes.Callvirt,
                            typeDefinition.Module.ImportReference(typeof(object).GetMethods().Single(m => m.Name == "GetHashCode"))));
                }
                else {
                    if (pkColDef.PropertyType.Name != typeof(Int32).Name) {
                        il.Add(Instruction.Create(OpCodes.Conv_I4));
                    }

                    il.Add(Instruction.Create(OpCodes.Stloc_1));
                    il.Add(Instruction.Create(OpCodes.Ldloca_S, var1));
                    il.Add(
                        Instruction.Create(
                            OpCodes.Call,
                            typeDefinition.Module.ImportReference(intTypeDef.Resolve().Methods.Single(m => m.Name == "GetHashCode"))));
                    il.Add(Instruction.Create(OpCodes.Ldc_I4, 17));
                    il.Add(Instruction.Create(OpCodes.Mul));
                }

                il.Add(Instruction.Create(OpCodes.Stloc_0));
                il.Add(endInstr);
                il.Add(Instruction.Create(OpCodes.Ret));
                typeDefinition.Methods.Add(method);
            }

            if (!this.DoesNotUseObjectMethod(typeDefinition, "Equals")) {
                // override equals
                var equals = new MethodDefinition(
                    "Equals",
                    Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig | Mono.Cecil.MethodAttributes.Virtual,
                    boolTypeDef);
                equals.Parameters.Add(new ParameterDefinition(objectTypeDef));
                equals.Body.InitLocals = true;
                equals.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                equals.Body.Variables.Add(new VariableDefinition(typeDefinition));
                var guidVar = new VariableDefinition(guidTypeDef);
                if (isGuidPk) {
                    equals.Body.Variables.Add(guidVar);
                }

                var il = equals.Body.Instructions;
                il.Add(Instruction.Create(OpCodes.Ldarg_1));
                var notNullInstr = Instruction.Create(OpCodes.Ldarg_1);
                il.Add(Instruction.Create(OpCodes.Brtrue_S, notNullInstr));
                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                var endInstr = Instruction.Create(OpCodes.Ldloc_0);
                il.Add(Instruction.Create(OpCodes.Br_S, endInstr));
                il.Add(notNullInstr);
                il.Add(Instruction.Create(OpCodes.Isinst, typeDefinition));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                il.Add(Instruction.Create(OpCodes.Ldloc_1));
                var nearlyTheEndInstr = Instruction.Create(OpCodes.Ldc_I4_0); // !!
                il.Add(Instruction.Create(OpCodes.Brfalse_S, nearlyTheEndInstr));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                if (isGuidPk) {
                    il.Add(Instruction.Create(OpCodes.Ldloca_S, guidVar));
                    il.Add(Instruction.Create(OpCodes.Initobj, guidTypeDef));
                    il.Add(Instruction.Create(OpCodes.Ldloc_2));
                    il.Add(
                        Instruction.Create(
                            OpCodes.Call,
                            typeDefinition.Module.ImportReference(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Inequality"))));
                }

                il.Add(Instruction.Create(OpCodes.Brfalse_S, nearlyTheEndInstr));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Ldloc_1));
                il.Add(Instruction.Create(OpCodes.Callvirt, pkColDef.GetMethod));
                var veryNearlyTheEndInstr = Instruction.Create(OpCodes.Stloc_0); // !!!
                if (isGuidPk) {
                    il.Add(
                        Instruction.Create(
                            OpCodes.Call,
                            typeDefinition.Module.ImportReference(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Equality"))));
                }
                else {
                    il.Add(Instruction.Create(OpCodes.Ceq));
                }

                il.Add(Instruction.Create(OpCodes.Br_S, veryNearlyTheEndInstr));
                il.Add(nearlyTheEndInstr);
                il.Add(veryNearlyTheEndInstr);
                il.Add(endInstr);
                il.Add(Instruction.Create(OpCodes.Ret));
                typeDefinition.Methods.Add(equals);
            }
        }
    }
}