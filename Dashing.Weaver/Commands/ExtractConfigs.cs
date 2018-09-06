namespace Dashing.Weaver.Commands {
    using System;
    using System.IO;
    using System.Linq;

    using Dashing.CommandLine;
    using Dashing.Weaver.ConfigurationMetadataGeneration;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;

    public static class ExtractConfigs {
        /// <summary>
        ///     loads an assembly, runs the IConfiguration and returns the map data
        /// </summary>
        public static void UseExtractConfigs(this CommandLineApplication app) {
            app.Command(
                "extractconfigs",
                c => {
                    c.Description = "Extracts meta data from the specified configurations";

                    var assemblyPath = c.Option("-p|--path <path>", "Specify the paths to the assemblies that you'd like to weave", CommandOptionType.MultipleValue)
                                        .IsRequired();
                    var configurationTypes = c.Option("-t|--typefullname <typefullname>", "The full names of the configuration types that describe the domain", CommandOptionType.MultipleValue)
                                              .IsRequired();
                    c.OnExecute(
                        () => {
                            AssemblyResolution.Configure(
                                assemblyPath.Values.Select(Path.GetDirectoryName)
                                            .ToArray());
                            var configurationMetadataGenerator = new ConfigurationMetadataGenerator();
                            try {
                                var metadata = configurationMetadataGenerator.GenerateMetadata(assemblyPath.Values, configurationTypes.Values);
                                Console.Write(metadata);
                            }
                            catch (Exception ex) {
                                Log.Logger.Fatal(ex, "extractconfigs failed unexpectedly");
                                return 1;
                            }

                            return 0;
                        });
                });
        }
    }
}