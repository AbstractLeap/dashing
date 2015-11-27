namespace Dashing.Console.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Console.Weaving.Weavers;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using Mono.Cecil;
    using Mono.Cecil.Pdb;

    using Newtonsoft.Json;

    using ILogger = Dashing.Tools.ILogger;

    [LoadInSeparateAppDomain]
    public class ExtendDomainTask : AppDomainIsolatedTask {
        public string WeaveDir { get; set; }

        public bool LaunchDebugger { get; set; }

        public bool IgnorePEVerify { get; set; }

        public ILogger Logger { get; set; }

        public override bool Execute() {
            if (this.LaunchDebugger) {
                Debugger.Launch();
            }

            // setup a logger to delegate to Log
            if (this.Logger == null) {
                if (this.Log != null) {
                    this.Logger = new TaskLoggingHelperLogger(this.Log);
                }
                else {
                    this.Logger = new NullLogger();
                }
            }

            var me = Assembly.GetExecutingAssembly();
            var peVerifier = new PEVerifier(this.Logger);

            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve(me);

            // load me in to a new app domain for creating IConfigurations
            this.Logger.Trace("Finding assemblies to weave in " + this.WeaveDir);
            var pathToDbm = new Uri(me.CodeBase).LocalPath;
            var configAppDomain = AppDomain.CreateDomain(
                "ConfigAppDomain",
                null,
                new AppDomainSetup { ApplicationBase = Path.GetDirectoryName(pathToDbm) });
            configAppDomain.AssemblyResolve += (sender, args) => {
                var assemblyName = new AssemblyName(args.Name);
                var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    return loaded;
                }

                // look in embedded resources
                var resourceName = "Dashing.Console.lib." + assemblyName.Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                    if (stream != null) {
                        var assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }

                // we couldn't find it, look on disk
                var path = Path.GetDirectoryName(assemblyName.Name + ".dll");
                if (File.Exists(path)) {
                    var assemblyData = File.ReadAllBytes(path);
                    return Assembly.Load(assemblyData);
                }

                return null;
            };

            var configurationMapResolver =
                (ConfigurationMapResolver)configAppDomain.CreateInstanceFromAndUnwrap(me.CodeBase, typeof(ConfigurationMapResolver).FullName);

            // locate all dlls
            var assemblyDefinitions = new Dictionary<string, AssemblyDefinition>();
            var assemblyMapDefinitions = new Dictionary<string, List<MapDefinition>>();
            foreach (var file in Directory.GetFiles(this.WeaveDir).Where(f => f.EndsWith("dll", StringComparison.InvariantCultureIgnoreCase) || f.EndsWith("exe", StringComparison.InvariantCultureIgnoreCase))) {
                try {
                    var readSymbols = File.Exists(file.Substring(0, file.Length - 3) + "pdb");
                    var assemblyResolver = new DefaultAssemblyResolver();
                    assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(file));
                    var assembly = AssemblyDefinition.ReadAssembly(
                        file,
                        new ReaderParameters { ReadSymbols = readSymbols, AssemblyResolver = assemblyResolver });
                    assemblyDefinitions.Add(file, assembly);
                    if (assembly.MainModule.AssemblyReferences.Any(a => a.Name == "Dashing")) {
                        this.Logger.Trace("Probing " + assembly.FullName + " for IConfigurations");

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

            this.Logger.Trace(
                "Found the following assemblies that reference dashing: " + string.Join(", ", assemblyMapDefinitions.Select(a => a.Key)));

            // now go through each assembly and re-write the types
            var visitedTypes = new HashSet<string>();
            var weavers = me.GetLoadableTypes().Where(t => typeof(IWeaver).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract).Select(
                t => {
                    var weaver = (IWeaver)Activator.CreateInstance(t);
                    ((ITaskLogHelper)weaver).Log = this.Logger;
                    return weaver;
                }).ToArray();

            this.Logger.Trace("Found the following weavers: " + string.Join(", ", weavers.Select(w => w.GetType().Name)));

            foreach (var assemblyMapDefinition in assemblyMapDefinitions) {
                var assemblyDefinitionLookup = assemblyDefinitions.Single(a => a.Value.FullName == assemblyMapDefinition.Key);
                var assemblyDefinition = assemblyDefinitionLookup.Value;
                foreach (var mapDefinition in assemblyMapDefinition.Value) {
                    if (visitedTypes.Contains(mapDefinition.TypeFullName)) {
                        continue;
                    }

                    this.Logger.Trace("Weaving {0} in {1}", mapDefinition.TypeFullName, mapDefinition.AssemblyFullName);
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
                    this.Logger.Trace(
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
                    if (!IgnorePEVerify) {
                        return false;
                    }
                }

                //// copy assembly back to its project location (for subsequent copies)
                //var projectFileLocations = new Queue<string>(new[] { this.BuildEngine.ProjectFileOfTaskNode, this.ProjectPath }.Where(s => !string.IsNullOrWhiteSpace(s)));
                //var processedFileLocations = new HashSet<string>();
                //while (projectFileLocations.Count > 0) {
                //    var projectFile = projectFileLocations.Dequeue();
                //    if (!processedFileLocations.Contains(projectFile)) {
                //        this.Log.LogMessage(
                //            MessageImportance.Normal,
                //            string.Format("Processing project file {0} and looking for referenced projects", projectFile));

                //        var csProj = this.GetProject(projectFile);
                //        if (csProj.GetPropertyValue("AssemblyName") != assemblyDefinition.Name.Name) {
                //            // if equal then this assembly is the one for this project so ignore
                //            var foundProject = this.FindProjectAndCopy(csProj, assemblyDefinition, assemblyDefinitionLookup.Key);
                //            if (foundProject) {
                //                break;
                //            }

                //            this.Log.LogMessage(
                //                MessageImportance.Normal,
                //                string.Format("Unable to find Project for {0} in {1}", assemblyDefinitionLookup.Key, projectFile));

                //            var parentProjectFile = csProj.GetPropertyValue("MSBuildThisFileFullPath"); // MSBUILD 4.0 only
                //            if (!string.IsNullOrWhiteSpace(parentProjectFile)) {
                //                projectFileLocations.Enqueue(parentProjectFile);
                //            }
                //        }

                //        processedFileLocations.Add(projectFile);
                //    }
                //}
            }

            return true;
        }

        private static ResolveEventHandler AssemblyResolve(Assembly me) {
            return (sender, args) => {
                var assemblyName = new AssemblyName(args.Name);
                var loaded = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    return loaded;
                }

                // look in embedded resources
                var resourceName = "Dashing.Console.lib." + assemblyName.Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                    if (stream != null) {
                        var assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }

                // we couldn't find it, look on disk
                var path = Path.GetDirectoryName(new Uri(me.CodeBase).LocalPath) + @"\" + assemblyName.Name + ".dll";
                if (File.Exists(path)) {
                    var assemblyData = File.ReadAllBytes(path);
                    return Assembly.Load(assemblyData);
                }

                return null;
            };
        }

        //private Project GetProject(string projectFilePath) {
        //    var loadedProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(Path.GetFullPath(projectFilePath));
        //    if (loadedProjects.Any()) {
        //        return loadedProjects.First();
        //    }

        //    return new Project(projectFilePath);
        //}

        //private bool FindProjectAndCopy(Project csProj, AssemblyDefinition assemblyDefinition, string filePath) {
        //    var projectReferences = csProj.Items.Where(i => i.ItemType == "ProjectReference").ToArray();

        //    // traverse down the project references until we find this dll or reach the bottom!
        //    foreach (var projectReference in projectReferences) {
        //        var thisProj = this.GetProject(Path.Combine(Path.GetDirectoryName(csProj.FullPath), projectReference.EvaluatedInclude));
        //        if (thisProj.GetPropertyValue("AssemblyName") == assemblyDefinition.Name.Name || this.FindProjectAndCopy(thisProj, assemblyDefinition, filePath)) {
        //            // bingo, it's this project, copy here
        //            var outputPath = thisProj.GetPropertyValue("OutputPath");
        //            var copyPath = Path.Combine(Path.GetDirectoryName(thisProj.FullPath), outputPath, Path.GetFileName(filePath));
        //            this.Log.LogMessage(MessageImportance.Normal, string.Format("Copying {0} to {1}", filePath, copyPath));
        //            File.Copy(filePath, copyPath, true);
        //            thisProj.ProjectCollection.UnloadProject(thisProj);
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}