namespace Dashing.Weaver.Weaving {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Dashing.Extensions;
    using Dashing.Weaver.Weaving.Weavers;

    using Mono.Cecil;

    using Newtonsoft.Json;

    public class ConfigurationWeaver {
        public bool Weave(IList<string> assemblyPaths, IList<string> configurationTypes) {
            var metadata = this.GetMetadata(assemblyPaths, configurationTypes);
            return Weave(metadata);
        }

        private static bool Weave(IList<MapDefinition> metadata) {
            var weavers = typeof(ConfigurationWeaver).Assembly()
                                                     .GetLoadableTypes()
                                                     .Where(t => typeof(IWeaver).IsAssignableFrom(t) && t.IsClass() && !t.IsAbstract())
                                                     .OrderBy(t => t.Name)
                                                     .Select(t => (IWeaver)Activator.CreateInstance(t))
                                                     .ToArray();
            foreach (var assemblyGroup in metadata.GroupBy(m => m.AssemblyPath)) {
                var hasSymbols = Path.HasExtension(Path.ChangeExtension(assemblyGroup.Key, "pdb"));
                var assemblyResolver = new DefaultAssemblyResolver();
                assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(assemblyGroup.Key));
                using (var assembly = AssemblyDefinition.ReadAssembly(
                    assemblyGroup.Key,
                    new ReaderParameters { ReadWrite = true, ReadSymbols = hasSymbols, AssemblyResolver = assemblyResolver })) {
                    foreach (var mapDefinition in assemblyGroup) {
                        var typeDefinition = assembly.MainModule.GetType(mapDefinition.TypeFullName);
                        foreach (var weaver in weavers) {
                            weaver.Weave(assembly, typeDefinition, mapDefinition.ColumnDefinitions);
                        }
                    }

                    assemblyResolver.Dispose();
                    assembly.Write();
                }
            }

            return true;
        }

        private IList<MapDefinition> GetMetadata(IEnumerable<string> assemblyPaths, IEnumerable<string> configurationTypes) {
            var arguments =
                $"-p {string.Join(",", assemblyPaths.Select(path => "\"" + path + "\""))} -t {string.Join(",", configurationTypes.Select(type => "\"" + type + "\""))}";
            var proc = new Process {
                                       StartInfo = new ProcessStartInfo {
#if COREFX
                                                                            FileName = "dotnet",
                                                                            Arguments = $"{typeof(ConfigurationWeaver).Assembly().Location} extractconfigs " + arguments,
#else
                                                                            FileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location),
                                                                            Arguments = "extractconfigs " + arguments,
#endif
                                                                            RedirectStandardOutput = true,
                                                                            RedirectStandardError = true,
                                                                            UseShellExecute = false,
                                                                            CreateNoWindow = true
                                                                        }
                                   };

            proc.Start();
            var stringBuilder = new StringBuilder();
            while (!proc.StandardOutput.EndOfStream) {
                stringBuilder.Append(proc.StandardOutput.ReadLine());
            }

            proc.WaitForExit();
            return JsonConvert.DeserializeObject<IList<MapDefinition>>(stringBuilder.ToString());
        }
    }
}