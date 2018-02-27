namespace Dashing.Weaver {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
#if COREFX
    using System.Runtime.Loader;
#endif

    using Dashing.Weaver.ConfigurationMetadataGeneration;
    using Dashing.Weaver.Weaving;

    using Microsoft.Extensions.CommandLineUtils;

    public class Program {
        public static int Main(string[] args) {
            var app = new CommandLineApplication {
                                                     Name = "dashing-weaver",
                                                     Description = "Weaves domain classes in order to support Dashing functionality"
                                                 };

            ConfigureExtractConfigs(app);
            ConfigureWeave(app);

            app.OnExecute(
                () =>
                    {
                        app.ShowHelp();
                        return 0;
                    });

            return app.Execute(args);
        }

        /// <summary>
        ///     The weave command modifies the domain classes using the specified configuration types in the specified assemblies
        ///     by first, executing the extractconfigs command to get the domain metadata, before weaving in the modified IL
        /// </summary>
        /// <param name="app"></param>
        private static void ConfigureWeave(CommandLineApplication app) {
            app.Command(
                "weave",
                c =>
                    {
                        c.Description = "Re-writes IL in the domain classes using the specified configurations";
                        c.HelpOption("-?|-h|--help");

                        // attempts to weave the assemblies at the specified location
                        var assemblyPath = c.Option(
                            "-p|--path <path>",
                            "Specify the paths to the assemblies that you'd like to weave",
                            CommandOptionType.MultipleValue);
                        var configurationTypes = c.Option(
                            "-t|--typefullname <typefullname>",
                            "The full names of the configuration types that describe the domain",
                            CommandOptionType.MultipleValue);

                        c.OnExecute(
                            () =>
                                {
                                    if (!assemblyPath.HasValue()) {
                                        Console.WriteLine("Please specify the path to the assemblies");
                                        return 1;
                                    }

                                    if (!configurationTypes.HasValue()) {
                                        Console.WriteLine("Please specify the configuration type full name(s)");
                                        return 1;
                                    }

                                    var configurationWeaver = new ConfigurationWeaver();
                                    try {
                                        var result = configurationWeaver.Weave(assemblyPath.Values, configurationTypes.Values);
                                        return result ? 0 : 1;
                                    }
                                    catch (Exception ex) {
                                        Console.WriteLine(ex.Message);
                                        return 1;
                                    }
                                });
                    });
        }

        /// <summary>
        ///     loads an assembly, runs the IConfiguration and returns the map data
        /// </summary>
        /// <param name="app"></param>
        private static void ConfigureExtractConfigs(CommandLineApplication app) {
            app.Command(
                "extractconfigs",
                c =>
                    {
                        c.Description = "Extracts meta data from the specified configurations";
                        c.HelpOption("-?|-h|--help");

                        var assemblyPath = c.Option(
                            "-p|--path <path>",
                            "Specify the paths to the assemblies that you'd like to weave",
                            CommandOptionType.MultipleValue);
                        var configurationTypes = c.Option(
                            "-t|--typefullname <typefullname>",
                            "The full names of the configuration types that describe the domain",
                            CommandOptionType.MultipleValue);
                        c.OnExecute(
                            () =>
                                {
                                    if (!assemblyPath.HasValue()) {
                                        Console.WriteLine("Please specify the path to the assemblies");
                                        return 1;
                                    }

                                    if (!configurationTypes.HasValue()) {
                                        Console.WriteLine("Please specify the configuration type full name(s)");
                                        return 1;
                                    }

                                    ConfigureAssemblyResolution(assemblyPath.Values);
                                    var configurationMetadataGenerator = new ConfigurationMetadataGenerator();
                                    try {
                                        var metadata =
                                            configurationMetadataGenerator.GenerateMetadata(assemblyPath.Values, configurationTypes.Values);
                                        Console.Write(metadata);
                                    }
                                    catch (Exception ex) {
                                        Console.WriteLine(ex.Message);
                                        return 1;
                                    }

                                    return 0;
                                });
                    });
        }

        private static void ConfigureAssemblyResolution(IEnumerable<string> assemblyPaths) {
            var pathsToSearch = assemblyPaths.Select(path => Path.GetDirectoryName(path)).ToArray();
#if COREFX
            AssemblyLoadContext.Default.Resolving += (context, name) =>
                {
                    // look on disk
                    foreach (var path in pathsToSearch) {
                        var attempts = new[] { "exe", "dll" }.Select(ext => $"{path}\\{name.Name}.{ext}");
                        foreach (var attempt in attempts) {
                            if (File.Exists(attempt)) {
                                return AssemblyContext.LoadFile(attempt);
                            }
                        }
                    }

                    return null;
                };
#else
            AppDomain.CurrentDomain.AssemblyResolve += (sender, iargs) =>
                {
                    var assemblyName = new AssemblyName(iargs.Name);

                    // look in app domain
                    var loaded = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == assemblyName.FullName);
                    if (loaded != null) {
                        return loaded;
                    }

                    // we couldn't find it, look on disk
                    foreach (var path in pathsToSearch) {
                        var attempts = new[] { "exe", "dll" }.Select(ext => $"{path}\\{assemblyName.Name}.{ext}");
                        foreach (var attempt in attempts) {
                            if (File.Exists(attempt)) {
                                return Assembly.LoadFile(attempt);
                                var assemblyData = File.ReadAllBytes(attempt);
                                return Assembly.Load(assemblyData);
                            }
                        }
                    }

                    return null;
                };
#endif
        }
    }
}