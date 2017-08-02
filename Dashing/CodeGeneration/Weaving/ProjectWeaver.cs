namespace Dashing.CodeGeneration.Weaving {
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Dashing.CodeGeneration.Weaving.Weavers;
    using Dashing.Configuration;

    using Mono.Cecil;

    using Newtonsoft.Json;

    public class ProjectWeaver {
        public static void Weave(string path, string assembly) {
            var assembliesToWeave = new List<string>();
            if (File.Exists(path)) {
                // they specified a precise path so we know what we're weaving
                assembliesToWeave.Add(path);
            }
            else {
                // they've specified a directory path, we'll iterate over it to find assemblies that match
                ProcessDirectory(path, assembly, assembliesToWeave);
            }

            if (assembliesToWeave.Any()) {
                // we need to fetch the map configurations for these types
                foreach (var assemblyPath in assembliesToWeave) {
                    var proc = new Process {
                                               StartInfo = new ProcessStartInfo {
#if COREFX
                        FileName = "dotnet",
                        Arguments = "dashing extractconfigs -p " + assemblyPath,
#else
                                                                                    FileName = "dotnet-dashing",
                                                                                    Arguments = "extractconfigs -p " + assemblyPath,
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
                    var mappings =
                        JsonConvert.DeserializeObject<Dictionary<string, IDictionary<string, IEnumerable<ColumnDefinition>>>>(
                            stringBuilder.ToString());
                    var peProcessor = new CecilPeProcessor(new ConsoleLogger(true));
                    foreach (var mapping in mappings) {
                        peProcessor.Process(assemblyPath, mapping.Value, new List<IWeaver>());
                    }
                }
            }
        }

        private static void ProcessDirectory(string path, string toWeave, List<string> assembliesToWeave) {
            foreach (var directory in Directory.GetDirectories(path)) {
                ProcessDirectory(directory, toWeave, assembliesToWeave);
            }

            if (!string.IsNullOrWhiteSpace(toWeave)) {
                var specifiedFilePath = Path.Combine(path, Path.GetFileName(toWeave));
                if (File.Exists(specifiedFilePath)) {
                    assembliesToWeave.Add(specifiedFilePath);
                }
            }
            else {
                foreach (var filePath in Directory.GetFiles(path).Where(f => f.EndsWith("dll") || f.EndsWith("exe"))) {
                    var assemblyResolver = new DefaultAssemblyResolver();
                    assemblyResolver.AddSearchDirectory(Path.GetDirectoryName(filePath));
                    var assembly = AssemblyDefinition.ReadAssembly(filePath, new ReaderParameters { AssemblyResolver = assemblyResolver });

                    if (assembly != null) {
                        if (assembly.MainModule.AssemblyReferences.Any(a => a.Name == "Dashing")
                            && assembly.MainModule.GetTypes().Any(t => t.ImplementsInterface(typeof(IConfiguration)))) {
                            assembliesToWeave.Add(filePath);
                        }
                    }
                }
            }
        }
    }
}