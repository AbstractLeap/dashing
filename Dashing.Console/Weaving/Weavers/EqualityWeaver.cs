namespace Dashing.Console.Weaving.Weavers {
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
            var guidTypeDef = typeDef.Module.Import(typeof(Guid));
            var boolTypeDef = typeDef.Module.Import(typeof(bool));
            var objectTypeDef = typeDef.Module.Import(typeof(object));
            var pkColDef = this.GetProperty(typeDef, mapDefinition.ColumnDefinitions.Single(d => d.IsPrimaryKey).Name);
            var isGuidPk = pkColDef.PropertyType.Name == "Guid";
            var isStringPk = !isGuidPk && pkColDef.PropertyType.Name.Equals("string", StringComparison.InvariantCultureIgnoreCase);

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

                var variableType = isGuidPk ? guidTypeDef : intTypeDef;
                method.Body.Variables.Add(new VariableDefinition(intTypeDef));
                var var1 = new VariableDefinition("CS$0$0000", variableType);
                var var2 = new VariableDefinition("CS$0$0001", variableType);
                if (!isStringPk) {
                    method.Body.Variables.Add(var1);
                }

                if (isGuidPk) {
                    method.Body.Variables.Add(var2);
                }

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
                        Instruction.Create(OpCodes.Call, typeDef.Module.Import(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Equality"))));
                    il.Add(Instruction.Create(OpCodes.Brfalse_S, useIdInstr));
                }
                else {
                    il.Add(Instruction.Create(OpCodes.Brtrue_S, useIdInstr));
                }

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
                il.Add(Instruction.Create(OpCodes.Br_S, endInstr));
                il.Add(useIdInstr);
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                if (isGuidPk) {
                    il.Add(Instruction.Create(OpCodes.Stloc_2));
                    il.Add(Instruction.Create(OpCodes.Ldloca_S, var2));
                    il.Add(Instruction.Create(OpCodes.Constrained, guidTypeDef));
                    il.Add(
                        Instruction.Create(OpCodes.Callvirt, typeDef.Module.Import(typeof(object).GetMethods().Single(m => m.Name == "GetHashCode"))));
                }
                else if (isStringPk) {
                    il.Add(
                        Instruction.Create(OpCodes.Callvirt, typeDef.Module.Import(typeof(object).GetMethods().Single(m => m.Name == "GetHashCode"))));
                }
                else {
                    if (pkColDef.PropertyType.Name != typeof(Int32).Name) {
                        il.Add(Instruction.Create(OpCodes.Conv_I4));
                    }

                    il.Add(Instruction.Create(OpCodes.Stloc_1));
                    il.Add(Instruction.Create(OpCodes.Ldloca_S, var1));
                    il.Add(Instruction.Create(OpCodes.Call, typeDef.Module.Import(intTypeDef.Resolve().Methods.Single(m => m.Name == "GetHashCode"))));
                    il.Add(Instruction.Create(OpCodes.Ldc_I4, 17));
                    il.Add(Instruction.Create(OpCodes.Mul));
                }

                il.Add(Instruction.Create(OpCodes.Stloc_0));
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
                equals.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                equals.Body.Variables.Add(new VariableDefinition(typeDef));
                var guidVar = new VariableDefinition("CS$0$0000", guidTypeDef);
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
                il.Add(Instruction.Create(OpCodes.Isinst, typeDef));
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
                        Instruction.Create(OpCodes.Call, typeDef.Module.Import(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Inequality"))));
                }

                il.Add(Instruction.Create(OpCodes.Brfalse_S, nearlyTheEndInstr));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Call, pkColDef.GetMethod));
                il.Add(Instruction.Create(OpCodes.Ldloc_1));
                il.Add(Instruction.Create(OpCodes.Callvirt, pkColDef.GetMethod));
                var veryNearlyTheEndInstr = Instruction.Create(OpCodes.Stloc_0); // !!!
                if (isGuidPk) {
                    il.Add(
                        Instruction.Create(OpCodes.Call, typeDef.Module.Import(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Equality"))));
                }
                else {
                    il.Add(Instruction.Create(OpCodes.Ceq));
                }

                il.Add(Instruction.Create(OpCodes.Br_S, veryNearlyTheEndInstr));
                il.Add(nearlyTheEndInstr);
                il.Add(veryNearlyTheEndInstr);
                il.Add(endInstr);
                il.Add(Instruction.Create(OpCodes.Ret));
                typeDef.Methods.Add(equals);
            }

            //// NOTE: See Dashing.Weaving.Test.EqualityTests.EqualityGetsOverridden

            //if (!this.DoesNotUseObjectMethod(typeDef, "op_Equality")) {
            //    var opEquality = new MethodDefinition("op_Equality", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static, boolTypeDef);
            //    opEquality.Parameters.Add(new ParameterDefinition(typeDef));
            //    opEquality.Parameters.Add(new ParameterDefinition(typeDef));
            //    var il = opEquality.Body.Instructions;
            //    il.Add(Instruction.Create(OpCodes.Ldarg_0));
            //    il.Add(Instruction.Create(OpCodes.Ldarg_1));
            //    il.Add(Instruction.Create(OpCodes.Call, typeDef.Module.Import(objectTypeDef.Resolve().Methods.Single(m => m.Name == "ReferenceEquals"))));
            //    var nullCheck = Instruction.Create(OpCodes.Ldarg_0);
            //    il.Add(Instruction.Create(OpCodes.Brfalse_S, nullCheck));
            //    il.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            //    il.Add(Instruction.Create(OpCodes.Ret));
            //    il.Add(nullCheck);
            //    var isNull = Instruction.Create(OpCodes.Ldc_I4_0);
            //    il.Add(Instruction.Create(OpCodes.Brfalse_S, isNull));
            //    il.Add(Instruction.Create(OpCodes.Ldarg_1));
            //    var checkIds = Instruction.Create(OpCodes.Ldarg_0);
            //    il.Add(Instruction.Create(OpCodes.Brtrue_S, checkIds));
            //    il.Add(isNull);
            //    il.Add(Instruction.Create(OpCodes.Ret));
            //    il.Add(checkIds);
            //    il.Add(Instruction.Create(OpCodes.Callvirt, pkColDef.GetMethod));
            //    il.Add(Instruction.Create(OpCodes.Ldarg_1));
            //    il.Add(Instruction.Create(OpCodes.Callvirt, pkColDef.GetMethod));
            //    if (isGuidPk) {
            //        il.Add(
            //            Instruction.Create(OpCodes.Call, typeDef.Module.Import(guidTypeDef.Resolve().Methods.Single(m => m.Name == "op_Equality"))));
            //    }
            //    else {
            //        il.Add(Instruction.Create(OpCodes.Ceq));
            //    }

            //    il.Add(Instruction.Create(OpCodes.Ret));
            //    typeDef.Methods.Add(opEquality);

            //    var opInequality = new MethodDefinition("op_Inequality", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Static, boolTypeDef);
            //    opInequality.Parameters.Add(new ParameterDefinition(typeDef));
            //    opInequality.Parameters.Add(new ParameterDefinition(typeDef));
            //    il = opInequality.Body.Instructions;
            //    il.Add(Instruction.Create(OpCodes.Ldarg_0));
            //    il.Add(Instruction.Create(OpCodes.Ldarg_1));
            //    il.Add(Instruction.Create(OpCodes.Call, opEquality));
            //    il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            //    il.Add(Instruction.Create(OpCodes.Ceq));
            //    il.Add(Instruction.Create(OpCodes.Ret));
            //    typeDef.Methods.Add(opInequality);
            //}
        }
    }
}