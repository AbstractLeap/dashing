namespace Dashing.Cli
{
    using System;

    using Dashing.Cli.Weaving;
    
    using Microsoft.Extensions.CommandLineUtils;

    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication { Name = "dashing", Description = "Provides database migrations as well as IL re-writing" };

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

        private static void ConfigureWeave(CommandLineApplication app)
        {
            app.Command(
                "weave",
                c =>
                    {
                        // attempts to weave the assemblies at the specified location
                        var path = c.Option("-p|--path <path>", "Specify the path to the bin folder or to a specific assembly that you'd like to weave", CommandOptionType.SingleValue);
                        var assembly = c.Option(
                            "-a|--assembly <assembly>",
                            "The file name of the assembly that you would like to weave (optional)",
                            CommandOptionType.SingleValue);

                        c.OnExecute(
                            () =>
                                {
                                    if (!path.HasValue())
                                    {
                                        Console.WriteLine("Please specify the path to the dll");
                                        return 1;
                                    }

                                    // go find assemblies to attempt weaving on 
                                    ProjectWeaver.Weave(path.Value(), assembly.Value());
                                    return 0;
                                });
                    });
        }

        private static void ConfigureExtractConfigs(CommandLineApplication app)
        {
            app.Command(
                "extractconfigs",
                c =>
                    {
                        // loads an assembly, runs the IConfiguration and returns the map data
                        var path = c.Option("-p|--path <path>", "Specify the path to the assembly containing the IConfiguration", CommandOptionType.SingleValue);
                        c.OnExecute(
                            () =>
                                {
                                    if (!path.HasValue())
                                    {
                                        Console.WriteLine("Please specify the path to the dll");
                                        return 1;
                                    }

                                    var configMapResolver = new ConfigurationMapResolver();
                                    configMapResolver.Resolve(path.Value());
                                    return 0;
                                });
                    });
        }
    }
}