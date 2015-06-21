namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

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

            // load me in to a new app domain for creating IConfigurations
            var configAppDomain = AppDomain.CreateDomain(
                "ConfigAppDomain",
                null,
                new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase });
            var configurationMapResolver =
                (ConfigurationMapResolver)configAppDomain.CreateInstanceFromAndUnwrap(me.CodeBase, typeof(ConfigurationMapResolver).FullName);

            // locate all dlls
            var assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
            var assemblyMapDefinitions = new Dictionary<string, List<MapDefinition>>();
            foreach (var file in Directory.GetFiles(AssemblyLocation.Directory)) {
                try {
                    var assembly = AssemblyDefinition.ReadAssembly(file);
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
                catch {
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
            foreach (var assemblyMapDefinition in assemblyMapDefinitions) {
                var assemblyDefinitionLookup = assemblyDefinitions.Single(a => a.Value.FullName == assemblyMapDefinition.Key);
                var assemblyDefinition = assemblyDefinitionLookup.Value;
                foreach (var mapDefinition in assemblyMapDefinition.Value) {
                    var typeDef = assemblyDefinition.MainModule.Types.Single(t => t.FullName == mapDefinition.TypeFullName);
                    ImplementITrackedEntity(typeDef, assemblyDefinition);
                }

                assemblyDefinition.Write(assemblyDefinitionLookup.Key);

                // verify assembly
                if (!peVerifier.Verify(assemblyDefinitionLookup.Key)) {
                    return false;
                }
            }

            return true;
        }

        private static void ImplementITrackedEntity(TypeDefinition typeDef, AssemblyDefinition assemblyDefinition) {
            if (typeDef.Interfaces.Any(i => i.FullName == typeof(ITrackedEntity).FullName)) {
                // already processed
                return;
            }

            typeDef.Interfaces.Add(assemblyDefinition.MainModule.Import(typeof(ITrackedEntity)));
            typeDef.Fields.Add(new FieldDefinition("isTracking", FieldAttributes.Private, typeDef.Module.Import(typeof(bool))));
            //AddAutoProperty(typeDef, "IsTracking", typeof(bool));
            //AddAutoProperty(typeDef, "DirtyProperties", typeof(ISet<>).MakeGenericType(typeof(string)));
            //AddAutoProperty(typeDef, "OldValues", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)));
            //AddAutoProperty(typeDef, "NewValues", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(object)));
            //AddAutoProperty(typeDef, "AddedEntities", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))));
            //AddAutoProperty(typeDef, "DeletedEntities", typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(IList<>).MakeGenericType(typeof(object))));
        }

        private static void AddAutoProperty(TypeDefinition typeDefinition, string name, Type propertyType) {
            var propertyTypeReference = typeDefinition.Module.Import(propertyType);
            var voidTypeReference = typeDefinition.Module.Import(typeof(void));
            var propertyDefinition = new PropertyDefinition(name, PropertyAttributes.None, propertyTypeReference);
            var fieldDefinition = new FieldDefinition(string.Format("<{0}>k_BackingField", name), FieldAttributes.Private, propertyTypeReference);

            // getter
            var get = new MethodDefinition(
                "get_" + name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
                propertyTypeReference);
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, fieldDefinition));
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
            var inst = Instruction.Create(OpCodes.Ldloc_0);
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Br_S, inst));
            get.Body.Instructions.Add(inst);
            get.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            get.Body.Variables.Add(new VariableDefinition(fieldDefinition.FieldType));
            get.Body.InitLocals = true;
            get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            typeDefinition.Methods.Add(get);
            propertyDefinition.GetMethod = get;

            // setter
            var set = new MethodDefinition("set_" + name,
                                       MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final,
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

        private string peVerifyPath;

        public PEVerifier() {
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            this.windowsSdkDirectory = Path.Combine(programFilesPath, @"Microsoft SDKs\Windows");
            if (!Directory.Exists(this.windowsSdkDirectory)) {
                this.foundPeVerify = false;
                return;
            }
            
            this.peVerifyPath = Directory.EnumerateFiles(this.windowsSdkDirectory, "peverify.exe", SearchOption.AllDirectories)
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
                Arguments = string.Format("\"{0}\" /hresult /ignore=0x80070002", assemblyPath),
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