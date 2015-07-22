namespace Dashing.CodeGeneration.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.CodeGeneration.Weaving.Weavers;

    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using Mono.Cecil;
    using Mono.Cecil.Pdb;

    using Newtonsoft.Json;

    [LoadInSeparateAppDomain]
    public class ExtendDomain : AppDomainIsolatedTask {
        public string ProjectPath { get; set; }

        public bool LaunchDebugger { get; set; }

        public override bool Execute() {
            if (this.LaunchDebugger) {
                Debugger.Launch();
            }

            var me = Assembly.GetExecutingAssembly();
            var peVerifier = new PEVerifier(this.Log);

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
            var pathToBin = new Uri(me.CodeBase).LocalPath;
            this.Log.LogMessage(MessageImportance.Normal, "Finding assemblies to weave in " + pathToBin);
            var configAppDomain = AppDomain.CreateDomain(
                "ConfigAppDomain",
                null,
                new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(pathToBin) });
            var configurationMapResolver =
                (ConfigurationMapResolver)
                configAppDomain.CreateInstanceFromAndUnwrap(pathToBin, typeof(ConfigurationMapResolver).FullName);

            // locate all dlls
            var assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
            var assemblyMapDefinitions = new Dictionary<string, List<MapDefinition>>();
            foreach (var file in Directory.GetFiles(AssemblyLocation.Directory)) {
                try {
                    var readSymbols = File.Exists(file.Substring(0, file.Length - 3) + "pdb");
                    var assemblyResolver = new DefaultAssemblyResolver();
                    assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(file));
                    var assembly = AssemblyDefinition.ReadAssembly(file, new ReaderParameters { ReadSymbols = readSymbols, AssemblyResolver = assemblyResolver});
                    assemblyDefinitions.Add(file, assembly);
                    if (assembly.MainModule.AssemblyReferences.Any(a => a.Name == me.GetName().Name)) {
                        this.Log.LogMessage(MessageImportance.Normal, "Probing " + assembly.FullName + " for IConfigurations");

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

            this.Log.LogMessage(MessageImportance.Normal, "Found the following weavers: " + string.Join(", ", weavers.Select(w => w.GetType().Name)));

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

                // copy assembly back to its project location (for subsequent copies)
                var projectFileLocations = new Queue<string>(new[] { this.BuildEngine.ProjectFileOfTaskNode, this.ProjectPath }.Where(s => !string.IsNullOrWhiteSpace(s)));
                var processedFileLocations = new HashSet<string>();
                while (projectFileLocations.Count > 0) {
                    var projectFile = projectFileLocations.Dequeue();
                    if (!processedFileLocations.Contains(projectFile)) {
                        this.Log.LogMessage(
                            MessageImportance.Normal,
                            string.Format("Processing project file {0} and looking for referenced projects", projectFile));

                        var csProj = this.GetProject(projectFile);
                        if (csProj.GetPropertyValue("AssemblyName") != assemblyDefinition.Name.Name) {
                            // if equal then this assembly is the one for this project so ignore
                            var foundProject = FindProjectAndCopy(csProj, assemblyDefinition, assemblyDefinitionLookup.Key);
                            if (foundProject) {
                                break;
                            }

                            this.Log.LogMessage(
                                MessageImportance.Normal,
                                string.Format("Unable to find Project for {0} in {1}", assemblyDefinitionLookup.Key, projectFile));
                            
                            var parentProjectFile = csProj.GetPropertyValue("MSBuildThisFileFullPath"); // MSBUILD 4.0 only
                            if (!string.IsNullOrWhiteSpace(parentProjectFile)) {
                                projectFileLocations.Enqueue(parentProjectFile);
                            }
                        }

                        processedFileLocations.Add(projectFile);
                    }
                }
            }

            return true;
        }

        private Project GetProject(string projectFilePath) {
            var loadedProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(Path.GetFullPath(projectFilePath));
            if (loadedProjects.Any()) {
                return loadedProjects.First();
            }

            return new Project(projectFilePath);
        }

        private bool FindProjectAndCopy(Project csProj, AssemblyDefinition assemblyDefinition, string filePath) {
            var projectReferences = csProj.Items.Where(i => i.ItemType == "ProjectReference").ToArray();

            // traverse down the project references until we find this dll or reach the bottom!
            foreach (var projectReference in projectReferences) {
                var thisProj = this.GetProject(Path.Combine(Path.GetDirectoryName(csProj.FullPath), projectReference.EvaluatedInclude));
                if (thisProj.GetPropertyValue("AssemblyName") == assemblyDefinition.Name.Name || FindProjectAndCopy(thisProj, assemblyDefinition, filePath)) {
                    // bingo, it's this project, copy here
                    var outputPath = thisProj.GetPropertyValue("OutputPath");
                    var copyPath = Path.Combine(Path.GetDirectoryName(thisProj.FullPath), outputPath, Path.GetFileName(filePath));
                    this.Log.LogMessage(MessageImportance.Normal, string.Format("Copying {0} to {1}", filePath, copyPath));
                    File.Copy(filePath, copyPath, true);
                    thisProj.ProjectCollection.UnloadProject(thisProj);
                    return true;
                }
            }

            return false;
        }
    }
}