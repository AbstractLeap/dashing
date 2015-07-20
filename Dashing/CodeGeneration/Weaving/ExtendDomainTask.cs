namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Policy;

    using Dashing.Configuration;
    using Dashing.Extensions;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Pdb;
    using Mono.Cecil.Rocks;

    using Newtonsoft.Json;

    using FieldAttributes = Mono.Cecil.FieldAttributes;
    using MethodAttributes = Mono.Cecil.MethodAttributes;
    using ParameterAttributes = Mono.Cecil.ParameterAttributes;
    using PropertyAttributes = Mono.Cecil.PropertyAttributes;

    [LoadInSeparateAppDomain]
    public class ExtendDomain : AppDomainIsolatedTask {
        public override bool Execute() {
            var me = Assembly.GetExecutingAssembly();
            var peVerifier = new PEVerifier();

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                var assemblyName = new AssemblyName(args.Name);
                var loaded = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    return loaded;
                }

                // we couldn't find it, look on disk
                var path = Path.GetDirectoryName(new Uri(me.CodeBase).LocalPath) + @"\" + assemblyName.Name + ".dll";
                if (File.Exists(path)) {
                    var assemblyData = File.ReadAllBytes(path);
                    return Assembly.Load(assemblyData);
                }

                return null;
            };

            // load me in to a new app domain for creating IConfigurations
            var configAppDomain = AppDomain.CreateDomain(
                "ConfigAppDomain",
                null,
                new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(new Uri(me.CodeBase).LocalPath) });
            var configurationMapResolver = (ConfigurationMapResolver)configAppDomain.CreateInstanceFromAndUnwrap(new Uri(me.CodeBase).LocalPath, typeof(ConfigurationMapResolver).FullName);

            // locate all dlls
            var assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
            var assemblyMapDefinitions = new Dictionary<string, List<MapDefinition>>();
            foreach (var file in Directory.GetFiles(AssemblyLocation.Directory)) {
                try {
                    var readSymbols = File.Exists(file.Substring(0, file.Length - 3) + "pdb");
                    var assembly = AssemblyDefinition.ReadAssembly(file, new ReaderParameters { ReadSymbols = readSymbols });
                    assemblyDefinitions.Add(file, assembly);
                    if (assembly.MainModule.AssemblyReferences.Any(a => a.Name == me.GetName().Name)) {
                        // references dashing, use our other app domain to find the IConfig and instantiate it
                        var args = new ConfigurationMapResolverArgs { AssemblyFilePath = file };
                        configurationMapResolver.Resolve(args);
                        var definitions = JsonConvert.DeserializeObject<IEnumerable<MapDefinition>>(args.SerializedConfigurationMapDefinitions);
                        if (definitions.Any()) {
                            foreach (var mapDefinition in definitions) {
                                if (!assemblyMapDefinitions.ContainsKey(mapDefinition.AssemblyFullName)) {
                                    assemblyMapDefinitions.Add(mapDefinition.AssemblyFullName, new List<MapDefinition>());
                                }

                                assemblyMapDefinitions[mapDefinition.AssemblyFullName].Add(mapDefinition);
                            }
                        }
                    }
                }
                catch (BadImageFormatException) {
                    // swallow and carry on - prob not a managed file
                }
            }

            // now we can unload the appdomain
            AppDomain.Unload(configAppDomain);

            // trim the list of assembly definitions to only those we need
            assemblyDefinitions =
                assemblyDefinitions.Where(k => assemblyMapDefinitions.Select(mk => mk.Key).Contains(k.Value.FullName))
                                   .ToDictionary(k => k.Key, k => k.Value);

            this.Log.LogMessage(
                MessageImportance.Normal,
                "Found the following assemblies that reference dashing: " + string.Join(", ", assemblyMapDefinitions.Select(a => a.Key)));

            // now go through each assembly and re-write the types
            var visitedTypes = new HashSet<string>();
            foreach (var assemblyMapDefinition in assemblyMapDefinitions) {
                var assemblyDefinitionLookup = assemblyDefinitions.Single(a => a.Value.FullName == assemblyMapDefinition.Key);
                var assemblyDefinition = assemblyDefinitionLookup.Value;
                foreach (var mapDefinition in assemblyMapDefinition.Value) {
                    if (visitedTypes.Contains(mapDefinition.TypeFullName)) {
                        continue;
                    }

                    var typeDef = GetTypeDefFromFullName(mapDefinition.TypeFullName, assemblyDefinition);
                    ImplementITrackedEntity(typeDef, assemblyDefinition, mapDefinition);
                    ImplementISetLogger(typeDef, assemblyDefinition, mapDefinition);
                    AddForeignKeyBehaviour(typeDef, assemblyDefinition, mapDefinition, assemblyMapDefinitions, assemblyDefinitions);
                    OverrideEqualsAndGetHashCode(typeDef, mapDefinition);
                    InstantiateCollections(typeDef, assemblyDefinition, mapDefinition);
                    visitedTypes.Add(mapDefinition.TypeFullName);
                }

                try {
                    if (File.Exists(assemblyDefinitionLookup.Key.Substring(0, assemblyDefinitionLookup.Key.Length - 3) + "pdb")) {
                        assemblyDefinition.Write(
                            assemblyDefinitionLookup.Key,
                            new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PdbWriterProvider() });
                    }
                    else {
                        assemblyDefinition.Write(assemblyDefinitionLookup.Key);
                    }
                }
                catch (UnauthorizedAccessException) {
                    this.Log.LogMessage(MessageImportance.High, "Unable to write the pdb for assembly " + assemblyDefinition.FullName + " due to an UnauthorizedAccessException");
                    try {
                        assemblyDefinition.Write(assemblyDefinitionLookup.Key);
                    }
                    catch (Exception) {
                        return false; // bugger it's broke
                    }
                }

                // verify assembly
                if (!peVerifier.Verify(assemblyDefinitionLookup.Key)) {
                    return false;
                }
            }

            return true;
        }

        private void InstantiateCollections(TypeDefinition typeDef, AssemblyDefinition assemblyDefinition, MapDefinition mapDefinition) {
            var constructors = typeDef.GetConstructors().ToArray();
            foreach (var oneToManyColumnDefinition in mapDefinition.ColumnDefinitions.Where(c => c.Relationship == RelationshipType.OneToMany)) {
                var propDef = typeDef.Properties.Single(p => p.Name == oneToManyColumnDefinition.Name);
                if (propDef.SetMethod.CustomAttributes.Any(c => c.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName)) {
                    // auto prop - see if the prop set method is called in any of the constructors
                    if (!constructors.Any(c => c.Body.Instructions.Any(i => i.Operand != null && i.Operand.Equals(propDef.SetMethod)))) {
                        var constructor = constructors.First();
                        if (constructors.Length > 1) {
                            constructor = constructors.SingleOrDefault(s => !s.HasParameters);
                            if (constructor == null) {
                                this.Log.LogMessage(MessageImportance.High, "Type " + typeDef.FullName + " does not have a parameterless constructor for instantiating collections in");
                            }
                        }

                        var insertIdx = constructor.Body.Instructions.Count - 1;
                        constructor.Body.Instructions.Insert(insertIdx++, Instruction.Create(OpCodes.Nop));
                        constructor.Body.Instructions.Insert(insertIdx++, Instruction.Create(OpCodes.Ldarg_0));
                        constructor.Body.Instructions.Insert(insertIdx++, Instruction.Create(OpCodes.Newobj, MakeGeneric(typeDef.Module.Import(typeDef.Module.Import(typeof(List<>)).MakeGenericInstanceType(propDef.PropertyType).Resolve().GetConstructors().First(c => !c.HasParameters)), ((GenericInstanceType)propDef.PropertyType).GenericArguments.First())));
                        constructor.Body.Instructions.Insert(insertIdx, Instruction.Create(OpCodes.Call, propDef.SetMethod));
                    }
                }
                else {
                    // not an auto prop
                    throw new NotImplementedException();
                }
            }
        }

        private void ImplementISetLogger(TypeDefinition typeDef, AssemblyDefinition assemblyDefinition, MapDefinition mapDefinition) {
            if (typeDef.Interfaces.Any(i => i.FullName == typeof(ISetLogger).FullName)) {
                return;
            }

            typeDef.Interfaces.Add(assemblyDefinition.MainModule.Import(typeof(ISetLogger)));
            
            // some common type definitions
            var boolTypeDef = typeDef.Module.Import(typeof(bool));
            var stringTypeDef = typeDef.Module.Import(typeof(string));
            var listStringTypeDef = typeDef.Module.Import(typeof(List<>)).MakeGenericInstanceType(stringTypeDef);
            var nonPkCols = mapDefinition.ColumnDefinitions.Where(c => !c.IsPrimaryKey && c.Relationship != RelationshipType.OneToMany).ToList();

            // add the fields for tracking which properties are set
            // and insert the code in to the setter
            foreach (var columnDefinition in nonPkCols) {
                var isSetFieldDef = new FieldDefinition(string.Format("__{0}_IsSet", columnDefinition.Name), FieldAttributes.Private, boolTypeDef);
                typeDef.Fields.Add(isSetFieldDef);

                // assign true to this field
                var il = typeDef.Properties.Single(p => p.Name == columnDefinition.Name).SetMethod.Body.Instructions;
                il.Insert(0, Instruction.Create(OpCodes.Stfld, isSetFieldDef));
                il.Insert(0, Instruction.Create(OpCodes.Ldc_I4_1));
                il.Insert(0, Instruction.Create(OpCodes.Ldarg_0));
            }

            // implement the interface
            var getSetProperties = new MethodDefinition(
                "GetSetProperties",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeDef.Module.Import(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef));
            getSetProperties.Body.Variables.Add(new VariableDefinition("setProps", listStringTypeDef));
            getSetProperties.Body.Variables.Add(
                new VariableDefinition(typeDef.Module.Import(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef)));
            getSetProperties.Body.Variables.Add(new VariableDefinition(boolTypeDef));
            getSetProperties.Body.InitLocals = true;
            var instructions = getSetProperties.Body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            var listStringContruictor =
                MakeGeneric(typeDef.Module.Import(listStringTypeDef.Resolve().GetConstructors().First(c => !c.HasParameters && !c.IsStatic && c.IsPublic)), stringTypeDef);
            instructions.Add(Instruction.Create(OpCodes.Newobj, listStringContruictor));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));

            var breakToInstruction = Instruction.Create(nonPkCols.Count == 1 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            var addMethod = typeDef.Module.Import(listStringTypeDef.Resolve().Methods.Single(m => m.Name == "Add"));
            addMethod = MakeGeneric(addMethod, stringTypeDef);
            for (var i = 0; i < nonPkCols.Count; i++) {
                if (i == 0) {
                    instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                }

                instructions.Add(
                    Instruction.Create(
                        OpCodes.Ldfld,
                        typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsSet", nonPkCols.ElementAt(i).Name))));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Stloc_2));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_2));
                instructions.Add(Instruction.Create(OpCodes.Brtrue, breakToInstruction));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, nonPkCols.ElementAt(i).Name));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, addMethod));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(breakToInstruction);
                breakToInstruction = Instruction.Create(i == nonPkCols.Count - 2 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            }

            instructions.Add(Instruction.Create(OpCodes.Stloc_1));
            var retInstr = Instruction.Create(OpCodes.Ldloc_1);
            instructions.Add(Instruction.Create(OpCodes.Br, retInstr));
            instructions.Add(retInstr);
            instructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getSetProperties);
        }

        private static TypeDefinition GetTypeDefFromFullName(string typeFullName, AssemblyDefinition assemblyDefinition) {
            TypeDefinition typeDef;
            if (typeFullName.Contains('+')) {
                var types = typeFullName.Split('+');
                typeDef = assemblyDefinition.MainModule.Types.Single(t => t.FullName == types.First());
                for (var i = 1; i < types.Length; i++) {
                    typeDef = typeDef.NestedTypes.Single(t => t.Name == types.ElementAt(i));
                }
            }
            else {
                typeDef = assemblyDefinition.MainModule.Types.Single(t => t.FullName == typeFullName);
            }

            return typeDef;
        }

        private void OverrideEqualsAndGetHashCode(TypeDefinition typeDef, MapDefinition mapDefinition) {
                var intTypeDef = typeDef.Module.Import(typeof(int));
                var boolTypeDef = typeDef.Module.Import(typeof(bool));
            var objectTypeDef = typeDef.Module.Import(typeof(object));
                var pkColDef = typeDef.Properties.Single(p => p.Name == mapDefinition.ColumnDefinitions.Single(d => d.IsPrimaryKey).Name);
                
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
                il.Add(Instruction.Create(OpCodes.Newobj, MakeGeneric(typeDef.Module.Import(hashCodeBackingField.FieldType.Resolve().GetConstructors().First()), typeDef.Module.Import(typeof(int)))));
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

        private bool DoesNotUseObjectMethod(TypeDefinition typeDefinition, string methodName) {
            return typeDefinition.Methods.Any(m => m.Name == methodName)
                   || (typeDefinition.BaseType.FullName != typeof(object).FullName
                       && this.DoesNotUseObjectMethod(typeDefinition.BaseType.Resolve(), methodName));
        }

        private static void AddForeignKeyBehaviour(TypeDefinition typeDef, AssemblyDefinition assemblyDefinition, MapDefinition mapDefinition, Dictionary<string, List<MapDefinition>> assemblyMapDefinitions, Dictionary<string, AssemblyDefinition> assemblyDefinitions) {
            var boolTypeDef = typeDef.Module.Import(typeof(bool));
            foreach (var columnDef in mapDefinition.ColumnDefinitions.Where(c => c.Relationship == RelationshipType.ManyToOne || c.Relationship == RelationshipType.OneToOne)) {
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
                if (typeDef.Fields.Any(f => f.Name == columnDef.DbName)) {
                    continue; // already done something here!
                }

                typeDef.Fields.Add(fkField);
var propDef = typeDef.Properties.Single(p => p.Name == columnDef.Name);
                
                // override the set method to set to null
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Initobj, fkTypeReference));
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldflda, fkField));
                propDef.SetMethod.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));

                // override the get method to access this field if null and create a new instance
                propDef.GetMethod.Body.Variables.Clear();
                propDef.GetMethod.Body.Variables.Add(new VariableDefinition(propDef.PropertyType));
                propDef.GetMethod.Body.Variables.Add(new VariableDefinition(propDef.PropertyType));
                propDef.GetMethod.Body.Variables.Add(new VariableDefinition(boolTypeDef));
                propDef.GetMethod.Body.InitLocals = true;
                propDef.GetMethod.Body.Instructions.Clear();

                var backingField = typeDef.Fields.Single(f => f.Name == string.Format("<{0}>k__BackingField", columnDef.Name));
                var il = propDef.GetMethod.Body.Instructions;
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                var skipToEnd = Instruction.Create(OpCodes.Ldc_I4_1);
                il.Add(Instruction.Create(OpCodes.Brtrue, skipToEnd));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                if (fkPkType.IsValueType) {
                    il.Add(Instruction.Create(OpCodes.Ldflda, fkField));
                    il.Add(Instruction.Create(OpCodes.Call, MakeGeneric(typeDef.Module.Import(fkTypeReference.Resolve().GetMethods().Single(m => m.Name == "get_HasValue")), typeDef.Module.Import(fkPkType))));
                }
                else {
                    throw new NotImplementedException(); // need to do a null check
                }

                il.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                il.Add(Instruction.Create(OpCodes.Ceq));
                var nopInstr = Instruction.Create(OpCodes.Nop);
                il.Add(Instruction.Create(OpCodes.Br, nopInstr));
                il.Add(skipToEnd);
                il.Add(nopInstr);
                il.Add(Instruction.Create(OpCodes.Stloc_2));
                il.Add(Instruction.Create(OpCodes.Ldloc_2));
                var returnThis = Instruction.Create(OpCodes.Ldarg_0);
                il.Add(Instruction.Create(OpCodes.Brtrue, returnThis));
                il.Add(Instruction.Create(OpCodes.Nop));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Newobj, propDef.PropertyType.Resolve().GetConstructors().First()));
                il.Add(Instruction.Create(OpCodes.Stloc_0));
                il.Add(Instruction.Create(OpCodes.Ldloc_0));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Ldflda, fkField));
                il.Add(Instruction.Create(OpCodes.Call, typeDef.Module.Import(MakeGeneric(fkField.FieldType.Resolve().GetMethods().Single(m => m.Name == "get_Value"), typeDef.Module.Import(fkPkType)))));
                var fkMapDef = assemblyMapDefinitions.SelectMany(am => am.Value).First(m => m.TypeFullName == columnDef.TypeFullName);
                var assemblyDef = assemblyDefinitions.Single(ad => ad.Value.FullName == fkMapDef.AssemblyFullName).Value;
                var fkMapTypeRef = GetTypeDefFromFullName(columnDef.TypeFullName, assemblyDef);
                il.Add(Instruction.Create(OpCodes.Callvirt, fkMapTypeRef.Properties.Single(c => c.Name == fkMapDef.ColumnDefinitions.Single(cd => cd.IsPrimaryKey).Name).SetMethod));
                il.Add(Instruction.Create(OpCodes.Ldloc_0));
                il.Add(Instruction.Create(OpCodes.Stfld, backingField));
                il.Add(returnThis);
                il.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                il.Add(Instruction.Create(OpCodes.Stloc_1));
                var ldLoc = Instruction.Create(OpCodes.Ldloc_1);
                il.Add(Instruction.Create(OpCodes.Br, ldLoc));
                il.Add(ldLoc);
                il.Add(Instruction.Create(OpCodes.Ret));
            }
        }

        private static void ImplementITrackedEntity(TypeDefinition typeDef, AssemblyDefinition assemblyDefinition, MapDefinition mapDefinition) {
            if (typeDef.Interfaces.Any(i => i.FullName == typeof(ITrackedEntity).FullName)) {
                // already processed
                return;
            }

            typeDef.Interfaces.Add(assemblyDefinition.MainModule.Import(typeof(ITrackedEntity)));

            // some common type definitions
            var boolTypeDef = typeDef.Module.Import(typeof(bool));
            var voidTypeDef = typeDef.Module.Import(typeof(void));
            var stringTypeDef = typeDef.Module.Import(typeof(string));
            var listStringTypeDef = typeDef.Module.Import(typeof(List<>)).MakeGenericInstanceType(stringTypeDef);
            var objectTypeDef = typeDef.Module.Import(typeof(object));

            // some column names
            const string isTrackingName = "__isTracking";

            // add the fields
            var isTrackingField = new FieldDefinition(isTrackingName, FieldAttributes.Private, boolTypeDef);
            typeDef.Fields.Add(isTrackingField);
            var nonPkCols = mapDefinition.ColumnDefinitions.Where(c => !c.IsPrimaryKey && c.Relationship != RelationshipType.OneToMany).ToList();
            foreach (var columnDefinition in nonPkCols) {
                var propertyDefinition = typeDef.Properties.Single(p => p.Name == columnDefinition.Name);
                typeDef.Fields.Add(new FieldDefinition(string.Format("__{0}_IsDirty", columnDefinition.Name), FieldAttributes.Private, boolTypeDef));

                // handle other maps, strings, valuetype, valuetype?
                var oldValuePropType = propertyDefinition.PropertyType;
                if (columnDefinition.Relationship == RelationshipType.None && propertyDefinition.PropertyType.IsValueType
                    && propertyDefinition.PropertyType.Name != "Nullable`1") {
                    oldValuePropType = typeDef.Module.Import(typeof(Nullable<>)).MakeGenericInstanceType(oldValuePropType);
                        // use nullable value types
                }

                typeDef.Fields.Add(
                    new FieldDefinition(string.Format("__{0}_OldValue", columnDefinition.Name), FieldAttributes.Private, oldValuePropType));
            }

            
            // insert the instructions in to the setter
            foreach (var columnDefinition in nonPkCols) {
                var propertyDef = typeDef.Properties.Single(p => p.Name == columnDefinition.Name);
                var setter = typeDef.Methods.Single(m => m.Name == "set_" + columnDefinition.Name);
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
                setIntructions.Add(Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsDirty", columnDefinition.Name))));
                setIntructions.Add(Instruction.Create(OpCodes.Stloc_0));
                setIntructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                setIntructions.Add(Instruction.Create(OpCodes.Brtrue, endNopInstr));
                setIntructions.Add(Instruction.Create(OpCodes.Nop));
                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));

                var backingField = typeDef.Fields.Single(f => f.Name == string.Format("<{0}>k__BackingField", columnDefinition.Name));
                if (propertyDef.PropertyType.IsValueType) {
                    var isEnum = propertyDef.PropertyType.Resolve().IsEnum;
                    if (isEnum) {
                        setIntructions.Add(Instruction.Create(OpCodes.Ldfld, backingField));
                        setIntructions.Add(Instruction.Create(OpCodes.Box, propertyDef.PropertyType));
                    }
                    else {
                        setIntructions.Add(Instruction.Create(OpCodes.Ldflda, backingField));
                    }

                    setIntructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                    if (isEnum) {
                        setIntructions.Add(Instruction.Create(OpCodes.Box, propertyDef.PropertyType));
                        setIntructions.Add(Instruction.Create(OpCodes.Callvirt, typeDef.Module.Import(objectTypeDef.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters.First().ParameterType.Name.ToLowerInvariant() == "object"))));
                    }
                    else if (propertyDef.PropertyType.Name == "Nullable`1") {
                        setIntructions.Add(Instruction.Create(OpCodes.Box, backingField.FieldType));
                        setIntructions.Add(Instruction.Create(OpCodes.Constrained, backingField.FieldType));
                        setIntructions.Add(Instruction.Create(OpCodes.Callvirt, typeDef.Module.Import(objectTypeDef.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters.First().ParameterType.Name.ToLowerInvariant() == "object"))));
                    }
                    else {
                        setIntructions.Add(Instruction.Create(OpCodes.Call, typeDef.Module.Import(propertyDef.PropertyType.Resolve().Methods.Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters.First().ParameterType.Name.ToLowerInvariant() != "object"))));
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
                    if (propertyDef.PropertyType.Name.ToLowerInvariant() == "string") {
                        setIntructions.Add(Instruction.Create(OpCodes.Callvirt, typeDef.Module.Import(propertyDef.PropertyType.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters.First().ParameterType.Name.ToLowerInvariant() == "string"))));
                    }
                    else {
                        setIntructions.Add(Instruction.Create(OpCodes.Callvirt, typeDef.Module.Import(objectTypeDef.Resolve().GetMethods().Single(m => m.Name == "Equals" && m.Parameters.Count == 1 && m.Parameters.First().ParameterType.Name.ToLowerInvariant() == "object"))));
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
                if (columnDefinition.Relationship == RelationshipType.None && propertyDef.PropertyType.IsValueType && propertyDef.PropertyType.Name != "Nullable`1") {
                    setIntructions.Add(Instruction.Create(OpCodes.Newobj, MakeGeneric(typeDef.Module.Import(typeDef.Fields.Single(f => f.Name == string.Format("__{0}_OldValue", columnDefinition.Name)).FieldType.Resolve().GetConstructors().First()), propertyDef.PropertyType)));
                }

                setIntructions.Add(Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == string.Format("__{0}_OldValue", columnDefinition.Name))));
                setIntructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                setIntructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
                setIntructions.Add(Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsDirty", columnDefinition.Name))));
                setIntructions.Add(Instruction.Create(OpCodes.Nop));
                setIntructions.Add(endNopInstr);
                setIntructions.Reverse();
                foreach (var instruction in setIntructions) {
                    setIl.Insert(0, instruction);
                }
            }

            // implement the ITrackedEntity methods
            // EnableTracking
            var enableTracking = new MethodDefinition(
                "EnableTracking",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                voidTypeDef);
            enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
            enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == isTrackingName)));
            enableTracking.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(enableTracking);

            // DisableTracking
            var disableTracking = new MethodDefinition(
                "DisableTracking",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                voidTypeDef);
            var disableInstructions = disableTracking.Body.Instructions;
            disableInstructions.Add(Instruction.Create(OpCodes.Nop));
            disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            disableInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
            disableInstructions.Add(Instruction.Create(OpCodes.Stfld, isTrackingField));
            foreach (var col in nonPkCols) {
                var propDef = typeDef.Properties.Single(p => p.Name == col.Name);

                // reset isdirty
                disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                disableInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                disableInstructions.Add(Instruction.Create(OpCodes.Stfld, typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsDirty", col.Name))));

                // reset oldvalue
                disableInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                var oldValueField = typeDef.Fields.Single(f => f.Name == string.Format("__{0}_OldValue", col.Name));
                if (propDef.PropertyType.IsValueType) {
                    disableInstructions.Add(Instruction.Create(OpCodes.Ldflda, oldValueField));
                    disableInstructions.Add(Instruction.Create(OpCodes.Initobj, oldValueField.FieldType));
                }
                else {
                    disableInstructions.Add(Instruction.Create(OpCodes.Ldnull));
                    disableInstructions.Add(Instruction.Create(OpCodes.Stfld, oldValueField));
                }
            }

            disableInstructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(disableTracking);

            // IsTrackingEnabled
            var isTrackingEnabled = new MethodDefinition(
                "IsTrackingEnabled",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
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

            // GetDirtyProperties
            var getDirtyProperties = new MethodDefinition(
                "GetDirtyProperties",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                typeDef.Module.Import(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef));
            getDirtyProperties.Body.Variables.Add(new VariableDefinition("dirtyProps", listStringTypeDef));
            getDirtyProperties.Body.Variables.Add(
                new VariableDefinition(typeDef.Module.Import(typeof(IEnumerable<>)).MakeGenericInstanceType(stringTypeDef)));
            getDirtyProperties.Body.Variables.Add(new VariableDefinition(boolTypeDef));
            getDirtyProperties.Body.InitLocals = true;
            var instructions = getDirtyProperties.Body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Nop));
            var listStringContruictor =
                MakeGeneric(typeDef.Module.Import(listStringTypeDef.Resolve().GetConstructors().First(c => !c.HasParameters && !c.IsStatic && c.IsPublic)), stringTypeDef);
            instructions.Add(Instruction.Create(OpCodes.Newobj, listStringContruictor));
            instructions.Add(Instruction.Create(OpCodes.Stloc_0));

            var breakToInstruction = Instruction.Create(nonPkCols.Count == 1 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            var addMethod = typeDef.Module.Import(listStringTypeDef.Resolve().Methods.Single(m => m.Name == "Add"));
            addMethod = MakeGeneric(addMethod, stringTypeDef);
            for (var i = 0; i < nonPkCols.Count; i++) {
                if (i == 0) {
                    instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                }

                instructions.Add(
                    Instruction.Create(
                        OpCodes.Ldfld,
                        typeDef.Fields.Single(f => f.Name == string.Format("__{0}_IsDirty", nonPkCols.ElementAt(i).Name))));
                instructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                instructions.Add(Instruction.Create(OpCodes.Ceq));
                instructions.Add(Instruction.Create(OpCodes.Stloc_2));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_2));
                instructions.Add(Instruction.Create(OpCodes.Brtrue, breakToInstruction));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
                instructions.Add(Instruction.Create(OpCodes.Ldstr, nonPkCols.ElementAt(i).Name));
                instructions.Add(Instruction.Create(OpCodes.Callvirt, addMethod));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(Instruction.Create(OpCodes.Nop));
                instructions.Add(breakToInstruction);
                breakToInstruction = Instruction.Create(i == nonPkCols.Count - 2 ? OpCodes.Ldloc_0 : OpCodes.Ldarg_0);
            }

            instructions.Add(Instruction.Create(OpCodes.Stloc_1));
            var retInstr = Instruction.Create(OpCodes.Ldloc_1);
            instructions.Add(Instruction.Create(OpCodes.Br, retInstr));
            instructions.Add(retInstr);
            instructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getDirtyProperties);

            // GetOldValue
            var getOldValue = new MethodDefinition(
                "GetOldValue",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                objectTypeDef);
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
            var opEqualityRef = typeDef.Module.Import(typeof(string).GetMethods().Single(m => m.Name == "op_Equality"));
            for (var i = 0; i < nonPkCols.Count; i++) {
                // generate the switch bit
                getBodyInstructions.Add(Instruction.Create(OpCodes.Ldloc_1));
                getBodyInstructions.Add(Instruction.Create(OpCodes.Ldstr, nonPkCols.ElementAt(i).Name));
                getBodyInstructions.Add(Instruction.Create(OpCodes.Call, opEqualityRef));

                // generate the if bit
                var targetInstr = Instruction.Create(OpCodes.Ldarg_0);
                getBodyInstructions.Add(Instruction.Create(OpCodes.Brtrue, targetInstr));
                switchInstructions.Add(targetInstr);
                switchInstructions.Add(Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == String.Format("__{0}_IsDirty", nonPkCols.ElementAt(i).Name))));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldc_I4_0));
                switchInstructions.Add(Instruction.Create(OpCodes.Ceq));
                switchInstructions.Add(Instruction.Create(OpCodes.Stloc_2));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldloc_2));

                // generate the return bit
                var breakInstruction = Instruction.Create(OpCodes.Br, throwExceptionTarget);
                switchInstructions.Add(Instruction.Create(OpCodes.Brtrue, breakInstruction));
                switchInstructions.Add(Instruction.Create(OpCodes.Nop));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                switchInstructions.Add(Instruction.Create(OpCodes.Ldfld, typeDef.Fields.Single(f => f.Name == String.Format("__{0}_OldValue", nonPkCols.ElementAt(i).Name))));
                if (typeDef.Properties.Single(p => p.Name == nonPkCols.ElementAt(i).Name).PropertyType.IsValueType) {
                    switchInstructions.Add(Instruction.Create(OpCodes.Box, typeDef.Fields.Single(f => f.Name == String.Format("__{0}_OldValue", nonPkCols.ElementAt(i).Name)).FieldType));
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
            getBodyInstructions.Add(Instruction.Create(OpCodes.Ldstr, "Either the property doesn't exist or it's not dirty. Consult GetDirtyProperties first"));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Newobj, typeDef.Module.Import(typeof(ArgumentOutOfRangeException).GetConstructors().First(c => c.GetParameters().All(p => p.ParameterType == typeof(string)) && c.GetParameters().Count() == 2))));
            getBodyInstructions.Add(Instruction.Create(OpCodes.Throw));
            getBodyInstructions.Add(returnTarget);
            getBodyInstructions.Add(Instruction.Create(OpCodes.Ret));
            typeDef.Methods.Add(getOldValue);

            //AddAutoProperty(typeDef, "IsTracking", typeof(bool));
            //AddAutoProperty(typeDef, "DirtyProperties", typeof(ISet<>).MakeGenericType(typeof(string)));
            //AddAutoProperty(typeDef, "OldValues", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)));
            //AddAutoProperty(typeDef, "NewValues", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)));
            //AddAutoProperty(typeDef, "AddedEntities", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))));
            //AddAutoProperty(typeDef, "DeletedEntities", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))));
        }

        public static TypeReference MakeGenericType(TypeReference self, params TypeReference[] arguments) {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static MethodReference MakeGeneric(MethodReference self, params TypeReference[] arguments) {
            var reference = new MethodReference(self.Name, self.ReturnType) {
                DeclaringType = MakeGenericType(self.DeclaringType, arguments),
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

        private static void AddAutoProperty(TypeDefinition typeDefinition, string name, Type propertyType) {
            var propertyTypeReference = typeDefinition.Module.Import(propertyType);
            var voidTypeReference = typeDefinition.Module.Import(typeof(void));
            var propertyDefinition = new PropertyDefinition(name, PropertyAttributes.None, propertyTypeReference);
            var fieldDefinition = new FieldDefinition(string.Format("<{0}>k_BackingField", name), FieldAttributes.Private, propertyTypeReference);

            // getter
            var get = new MethodDefinition(
                "get_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot
                | MethodAttributes.Virtual | MethodAttributes.Final,
                propertyTypeReference);
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, fieldDefinition));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            var inst = Instruction.Create(OpCodes.Ldloc_0);
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Br, inst));
            get.Body.Instructions.Add(inst);
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            get.Body.Variables.Add(new VariableDefinition(fieldDefinition.FieldType));
            get.Body.InitLocals = true;
            get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            typeDefinition.Methods.Add(get);
            propertyDefinition.GetMethod = get;

            // setter
            var set = new MethodDefinition(
                "set_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot
                | MethodAttributes.Virtual | MethodAttributes.Final,
                voidTypeReference);
            var instructions = set.Body.Instructions;
            instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            instructions.Add(Instruction.Create(OpCodes.Stfld, fieldDefinition));
            instructions.Add(Instruction.Create(OpCodes.Ret));
            set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, fieldDefinition.FieldType));
            set.SemanticsAttributes = MethodSemanticsAttributes.Setter;
            //set.CustomAttributes.Add(new CustomAttribute(msCoreReferenceFinder.CompilerGeneratedReference));
            typeDefinition.Methods.Add(set);
            propertyDefinition.SetMethod = set;

            // add to type
            typeDefinition.Fields.Add(fieldDefinition);
            typeDefinition.Properties.Add(propertyDefinition);
        }
    }

    public class PEVerifier {
        private string windowsSdkDirectory;

        private bool foundPeVerify;

        private readonly string peVerifyPath;

        public PEVerifier() {
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            this.windowsSdkDirectory = Path.Combine(programFilesPath, @"Microsoft SDKs\Windows");
            if (!Directory.Exists(this.windowsSdkDirectory)) {
                this.foundPeVerify = false;
                return;
            }

            this.peVerifyPath =
                Directory.EnumerateFiles(this.windowsSdkDirectory, "peverify.exe", SearchOption.AllDirectories)
                         .Where(x => !x.ToLowerInvariant().Contains("x64"))
                         .OrderByDescending(x => FileVersionInfo.GetVersionInfo(x).FileVersion)
                         .FirstOrDefault();

            if (this.peVerifyPath == null) {
                this.foundPeVerify = false;
                return;
            }

            this.foundPeVerify = true;
        }

        public bool Verify(string assemblyPath) {
            var processStartInfo = new ProcessStartInfo(this.peVerifyPath) {
                                                                               Arguments =
                                                                                   string.Format("\"{0}\" /hresult /ignore=0x80070002", assemblyPath),
                                                                               WorkingDirectory = Path.GetDirectoryName(assemblyPath),
                                                                               CreateNoWindow = true,
                                                                               UseShellExecute = false,
                                                                               RedirectStandardOutput = true
                                                                           };

            using (var process = Process.Start(processStartInfo)) {
                var output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0) {
                    return false;
                }
            }
            return true;
        }
    }
}