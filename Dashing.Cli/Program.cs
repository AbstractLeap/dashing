namespace Dashing.Cli {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
#if COREFX
    using System.Runtime.Loader;
#endif
    
    using Microsoft.Extensions.CommandLineUtils;

    public class Program {
        public static int Main(string[] args) {
            var app = new CommandLineApplication {
                                                     Name = "dashing",
                                                     Description = "Provides functionality to migrate databases"
                                                 };
            
            ConfigureScript(app);

            app.OnExecute(
                () =>
                    {
                        app.ShowHelp();
                        return 0;
                    });

            return app.Execute(args);
        }
        
        private static void ConfigureScript(CommandLineApplication app) {
            app.Command(
                "script",
                c =>
                    {
                        c.Description = "Generates a script that will migrate an existing database so that it matches the specified configuration";
                        c.HelpOption("-?|-h|--help");

                        // attempts to weave the assemblies at the specified location
                        var assemblyPath = c.Option(
                            "-a|--assemblypath <path>",
                            "Specify the path to the assembly that contains the configuration",
                            CommandOptionType.SingleValue);
                        var configurationType = c.Option(
                            "-t|--typefullname <typefullname>",
                            "The full name of the configuration type that describes the domain",
                            CommandOptionType.SingleValue);
                        var connectionString = c.Option(
                            "-c|--connection <connectionstring>",
                            "The connection string of the database that you would like to migrate",
                            CommandOptionType.SingleValue);
                        var provider = c.Option(
                            "-p|--provider <providername>",
                            "The provider name for the database that you are migrating",
                            CommandOptionType.SingleValue);
                        var tablesToIgnore = c.Option(
                            "-ti|--tablestoignore <tablename>",
                            "The name of any tables which should be ignored in the migration",
                            CommandOptionType.MultipleValue);
                        var indexesToIgnore = c.Option(
                            "-ii|--indexestoignore <indexname>",
                            "The name of any indexes which should be ignored in the migration",
                            CommandOptionType.MultipleValue);
                        var extraPluralizationWords = c.Option(
                            "-ep|--extraplurals <singlename,pluralname>",
                            "Any extra single/plural pairs that need adding",
                            CommandOptionType.MultipleValue);
                        var verbose = c.Option(
                            "-v|--verbose",
                            "Outputs debug logging statements",
                            CommandOptionType.NoValue);

                        c.OnExecute(
                            () =>
                                {
                                    if (!assemblyPath.HasValue()) {
                                        Console.WriteLine("Please specify the path to the assembly");
                                        return 1;
                                    }

                                    if (!configurationType.HasValue()) {
                                        Console.WriteLine("Please specify the configuration type full name");
                                        return 1;
                                    }

                                    if (!connectionString.HasValue()) {
                                        Console.WriteLine("Please specify the connection string");
                                        return 1;
                                    }

                                    ConfigureAssemblyResolution(assemblyPath.Value());
                                    DisplayMigrationHeader(assemblyPath.Value(), configurationType.Value());
                                    var configurationWeaver = new ConfigurationWeaver();
                                    try {
                                        var result = configurationWeaver.Weave(assemblyPath.Values, configurationType.Values);
                                        return result ? 0 : 1;
                                    }
                                    catch (Exception ex) {
                                        Console.WriteLine(ex.Message);
                                        return 1;
                                    }
                                });
                    });
        }

        private static void DisplayMigrationHeader(string assemblyPath, string configurationFullName)
        {
            using (Color(ConsoleColor.Yellow))
            {
                Console.WriteLine("-- Dashing: Migration Script");
            }

            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", assemblyPath);
            Console.WriteLine("-- Class:    {0}", configurationFullName);
            Console.WriteLine("-- ");
            
            using (Color(ConsoleColor.Yellow))
            {
                Console.WriteLine("-- -------------------------------");
                Console.WriteLine("-- Migration is experimental:");
                Console.WriteLine("-- Please check the output!");
                Console.WriteLine("-- -------------------------------");
            }
        }

        private static ColorContext Color(ConsoleColor color)
        {
            return new ColorContext(color);
        }

        private static void ConfigureAssemblyResolution(string assemblyPath) {
            var assemblyDir = Path.GetDirectoryName(assemblyPath);
#if COREFX
            AssemblyLoadContext.Default.Resolving += (context, name) =>
                {
                    // look on disk
                    var attempts = new[] { "exe", "dll" }.Select(ext => $"{assemblyDir}\\{name.Name}.{ext}");
                    foreach (var attempt in attempts) {
                        if (File.Exists(attempt)) {
                            return AssemblyContext.LoadFile(attempt);
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
                    var attempts = new[] { "exe", "dll" }.Select(ext => $"{assemblyDir}\\{assemblyName.Name}.{ext}");
                    foreach (var attempt in attempts) {
                        if (File.Exists(attempt)) {
                            return Assembly.LoadFile(attempt);
                        }
                    }

                    return null;
                };
#endif
        }
    }
}