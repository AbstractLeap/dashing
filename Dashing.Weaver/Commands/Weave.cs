namespace Dashing.Weaver.Commands {
    using System;

    using Dashing.Weaver.Weaving;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;

    public static class Weave {
        /// <summary>
        ///     The weave command modifies the domain classes using the specified configuration types in the specified assemblies
        ///     by first, executing the extractconfigs command to get the domain metadata, before weaving in the modified IL
        /// </summary>
        public static void UseWeave(this CommandLineApplication app) {
            app.Command(
                "weave",
                c => {
                    c.Description = "Re-writes IL in the domain classes using the specified configurations";

                    // attempts to weave the assemblies at the specified location
                    var assemblyPath = c.Option("-p|--path <path>", "Specify the paths to the assemblies that you'd like to weave", CommandOptionType.MultipleValue)
                                        .IsRequired();
                    var configurationTypes = c.Option("-t|--typefullname <typefullname>", "The full names of the configuration types that describe the domain", CommandOptionType.MultipleValue)
                                              .IsRequired();

                    c.OnExecute(
                        () => {
                            var configurationWeaver = new ConfigurationWeaver();
                            try {
                                var result = configurationWeaver.Weave(assemblyPath.Values, configurationTypes.Values);
                                return result
                                           ? 0
                                           : 1;
                            }
                            catch (Exception ex) {
                                Log.Logger.Fatal(ex, "weave failed unexpectedly");
                                return 1;
                            }
                        });
                });
        }
    }
}