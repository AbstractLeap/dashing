namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration.Weaving.Weavers;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using Mono.Cecil;
    using Mono.Cecil.Pdb;

    using Newtonsoft.Json;

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
            var configurationMapResolver =
                (ConfigurationMapResolver)
                configAppDomain.CreateInstanceFromAndUnwrap(new Uri(me.CodeBase).LocalPath, typeof(ConfigurationMapResolver).FullName);

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
            var weavers = me.DefinedTypes.Where(t => typeof(IWeaver).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).Select(
                t => {
                    var weaver = (IWeaver)Activator.CreateInstance(t);
                    ((ITaskLogHelper)weaver).Log = this.Log;
                    return weaver;
                }).ToArray();

            foreach (var assemblyMapDefinition in assemblyMapDefinitions) {
                var assemblyDefinitionLookup = assemblyDefinitions.Single(a => a.Value.FullName == assemblyMapDefinition.Key);
                var assemblyDefinition = assemblyDefinitionLookup.Value;
                foreach (var mapDefinition in assemblyMapDefinition.Value) {
                    if (visitedTypes.Contains(mapDefinition.TypeFullName)) {
                        continue;
                    }

                    this.Log.LogMessage(
                        MessageImportance.Normal,
                        string.Format("Weaving {0} in {1}", mapDefinition.TypeFullName, mapDefinition.AssemblyFullName));
                    var typeDef = BaseWeaver.GetTypeDefFromFullName(mapDefinition.TypeFullName, assemblyDefinition);
                    foreach (var weaver in weavers) {
                        weaver.Weave(typeDef, assemblyDefinition, mapDefinition, assemblyMapDefinitions, assemblyDefinitions);
                    }

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
                    this.Log.LogMessage(
                        MessageImportance.High,
                        "Unable to write the pdb for assembly " + assemblyDefinition.FullName + " due to an UnauthorizedAccessException");
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
    }
}