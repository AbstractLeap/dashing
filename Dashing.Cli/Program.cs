namespace Dashing.Cli {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Dashing.CommandLine;
    using Dashing.Configuration;
    using Dashing.Extensions;

    using Microsoft.Extensions.CommandLineUtils;
#if !COREFX
    using System.Configuration;
#endif

    public class Program {
#if COREFX
        private static readonly IList<string> AssemblySearchDirectories = new List<string>();
#else
        private static readonly IList<string> AssemblySearchDirectories = (ConfigurationManager.AppSettings["AssemblySearchPaths"]
                                                                                               ?.Split(';') ?? Enumerable.Empty<string>()).ToList();
#endif

        public static int Main(string[] args) {
            AssemblyResolution.Configure(AssemblySearchDirectories); // we have to configure the assembly resolution on it's own in this method as the ExecuteApplication needs it
            return ExecuteApplication(args);
        }

        private static int ExecuteApplication(string[] args) {
            var app = new CommandLineApplication {
                                                     Name = "dash",
                                                     Description = "Provides functionality to migrate databases"
                                                 };
            ConfigureScript(app);
            ConfigureMigrate(app);
            ConfigureSeed(app);
            ConfigureAddWeave(app);

            app.OnExecute(
                () => {
                    app.ShowHelp();
                    return 0;
                });

            return app.Execute(args);
        }

        private static void ConfigureScript(CommandLineApplication app) {
            app.Command(
                "script",
                c => {
                    c.Description = "Generates a script that will migrate an existing database so that it matches the specified configuration";
                    c.HelpOption("-?|-h|--help");

                    // attempts to weave the assemblies at the specified location
                    var assemblyPath = c.Option("-a|--assemblypath <path>", "Specify the path to the assembly that contains the configuration", CommandOptionType.SingleValue);
                    var configurationType = c.Option("-t|--typefullname <typefullname>", "The full name of the configuration type that describes the domain", CommandOptionType.SingleValue);
                    var connectionString = c.Option("-c|--connection <connectionstring>", "The connection string of the database that you would like to migrate", CommandOptionType.SingleValue);
                    var provider = c.Option("-p|--provider <providername>", "The provider name for the database that you are migrating", CommandOptionType.SingleValue);
                    var tablesToIgnore = c.Option("-ti|--tablestoignore <tablename>", "The name of any tables which should be ignored in the migration", CommandOptionType.MultipleValue);
                    var indexesToIgnore = c.Option("-ii|--indexestoignore <indexname>", "The name of any indexes which should be ignored in the migration", CommandOptionType.MultipleValue);
                    var extraPluralizationWords = c.Option("-ep|--extraplurals <singlename,pluralname>", "Any extra single/plural pairs that need adding", CommandOptionType.MultipleValue);
                    var logLevel = c.Option("-l|--log <level>", "The logging level, one of verbose,debug,information,warning,error,fatal. Defaults to warning", CommandOptionType.SingleValue);

                    c.OnExecute(
                        () => {
                            LogConfiguration.Configure(logLevel);
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

                            var assemblyDir = Path.GetDirectoryName(assemblyPath.Value());
                            AssemblySearchDirectories.Insert(0, assemblyDir); // favour user code over dashing code
                            DisplayMigrationHeader(assemblyPath.Value(), configurationType.Value(), connectionString.Value());
                            try {
                                ExecuteScript(assemblyPath, configurationType, connectionString, provider, tablesToIgnore, indexesToIgnore, extraPluralizationWords);
                                return 0;
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                                Console.Write(ex.StackTrace);
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteScript(CommandOption assemblyPath, CommandOption configurationType, CommandOption connectionString, CommandOption provider, CommandOption tablesToIgnore, CommandOption indexesToIgnore, CommandOption extraPluralizationWords) {
            var scriptGenerator = new ScriptGenerator();
            var result = scriptGenerator.Generate(
                LoadType<IConfiguration>(assemblyPath.Value(), configurationType.Value()),
                connectionString.Value(),
                provider.HasValue()
                    ? provider.Value()
                    : "System.Data.SqlClient",
                tablesToIgnore.Values,
                indexesToIgnore.Values,
                GetExtraPluralizationWords(extraPluralizationWords),
                new ConsoleAnswerProvider());
            Console.Write(result);
        }

        private static void ConfigureMigrate(CommandLineApplication app) {
            app.Command(
                "migrate",
                c => {
                    c.Description = "Generates and runs a script that will migrate an existing database so that it matches the specified configuration";
                    c.HelpOption("-?|-h|--help");

                    // attempts to weave the assemblies at the specified location
                    var assemblyPath = c.Option("-a|--assemblypath <path>", "Specify the path to the assembly that contains the configuration", CommandOptionType.SingleValue);
                    var configurationType = c.Option("-t|--typefullname <typefullname>", "The full name of the configuration type that describes the domain", CommandOptionType.SingleValue);
                    var connectionString = c.Option("-c|--connection <connectionstring>", "The connection string of the database that you would like to migrate", CommandOptionType.SingleValue);
                    var provider = c.Option("-p|--provider <providername>", "The provider name for the database that you are migrating", CommandOptionType.SingleValue);
                    var tablesToIgnore = c.Option("-ti|--tablestoignore <tablename>", "The name of any tables which should be ignored in the migration", CommandOptionType.MultipleValue);
                    var indexesToIgnore = c.Option("-ii|--indexestoignore <indexname>", "The name of any indexes which should be ignored in the migration", CommandOptionType.MultipleValue);
                    var extraPluralizationWords = c.Option("-ep|--extraplurals <singlename,pluralname>", "Any extra single/plural pairs that need adding", CommandOptionType.MultipleValue);
                    var logLevel = c.Option("-l|--log <level>", "The logging level, one of verbose,debug,information,warning,error,fatal. Defaults to warning", CommandOptionType.SingleValue);

                    c.OnExecute(
                        () => {
                            LogConfiguration.Configure(logLevel);
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

                            var assemblyDir = Path.GetDirectoryName(assemblyPath.Value());
                            AssemblySearchDirectories.Insert(0, assemblyDir); // favour user code over dashing code
                            DisplayMigrationHeader(assemblyPath.Value(), configurationType.Value(), connectionString.Value());
                            try {
                                ExecuteMigrate(assemblyPath, configurationType, connectionString, provider, tablesToIgnore, indexesToIgnore, extraPluralizationWords);
                                return 0;
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteMigrate(CommandOption assemblyPath, CommandOption configurationType, CommandOption connectionString, CommandOption provider, CommandOption tablesToIgnore, CommandOption indexesToIgnore, CommandOption extraPluralizationWords) {
            var databaseMigrator = new DatabaseMigrator();
            databaseMigrator.Execute(
                LoadType<IConfiguration>(assemblyPath.Value(), configurationType.Value()),
                connectionString.Value(),
                provider.HasValue()
                    ? provider.Value()
                    : "System.Data.SqlClient",
                tablesToIgnore.Values,
                indexesToIgnore.Values,
                GetExtraPluralizationWords(extraPluralizationWords),
                new ConsoleAnswerProvider());
        }

        private static void ConfigureSeed(CommandLineApplication app) {
            app.Command(
                "seed",
                c => {
                    c.Description = "Executes a function to seed a database";
                    c.HelpOption("-?|-h|--help");

                    // attempts to weave the assemblies at the specified location
                    var configurationAssemblyPath = c.Option("-ca|--configurationassemblypath <path>", "Specify the path to the assembly that contains the configuration", CommandOptionType.SingleValue);
                    var configurationType = c.Option("-ct|--typefullname <typefullname>", "The full name of the configuration type", CommandOptionType.SingleValue);
                    var seederAssemblyPath = c.Option("-sa|--configurationassemblypath <path>", "Specify the path to the assembly that contains the seeder", CommandOptionType.SingleValue);
                    var seederType = c.Option("-st|--typefullname <typefullname>", "The full name of the seeder type", CommandOptionType.SingleValue);
                    var connectionString = c.Option("-c|--connection <connectionstring>", "The connection string of the database that you would like to migrate", CommandOptionType.SingleValue);
                    var provider = c.Option("-p|--provider <providername>", "The provider name for the database that you are migrating", CommandOptionType.SingleValue);
                    var logLevel = c.Option("-l|--log <level>", "The logging level, one of verbose,debug,information,warning,error,fatal. Defaults to warning", CommandOptionType.SingleValue);

                    c.OnExecute(
                        () => {
                            LogConfiguration.Configure(logLevel);
                            if (!configurationAssemblyPath.HasValue()) {
                                Console.WriteLine("Please specify the path to the assembly that contains the configuration");
                                return 1;
                            }

                            if (!configurationType.HasValue()) {
                                Console.WriteLine("Please specify the configuration full name");
                                return 1;
                            }

                            if (!seederAssemblyPath.HasValue()) {
                                Console.WriteLine("Please specify the path to the assembly that contains the seeder");
                                return 1;
                            }

                            if (!seederType.HasValue()) {
                                Console.WriteLine("Please specify the seeder full name");
                                return 1;
                            }

                            if (!connectionString.HasValue()) {
                                Console.WriteLine("Please specify the connection string");
                                return 1;
                            }

                            var assemblyDir = Path.GetDirectoryName(configurationAssemblyPath.Value());
                            AssemblySearchDirectories.Insert(0, assemblyDir); // favour user code over dashing code
                            try {
                                ExecuteSeed(seederAssemblyPath, seederType, configurationAssemblyPath, configurationType, connectionString, provider);
                                return 0;
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteSeed(CommandOption seederAssemblyPath, CommandOption seederType, CommandOption configurationAssemblyPath, CommandOption configurationType, CommandOption connectionString, CommandOption provider) {
            var seeder = new Seeder();
            seeder.Execute(
                LoadType<ISeeder>(seederAssemblyPath.Value(), seederType.Value()),
                LoadType<IConfiguration>(configurationAssemblyPath.Value(), configurationType.Value()),
                connectionString.Value(),
                provider.HasValue()
                    ? provider.Value()
                    : "System.Data.SqlClient");
        }

        private static void ConfigureAddWeave(CommandLineApplication app) {
            app.Command(
                "addweave",
                c => {
                    c.Description = "Executes a function to add weaving to a project";
                    c.HelpOption("-?|-h|--help");

                    // attempts to weave the assemblies at the specified location
                    var projectFilePath = c.Option("-p|--projectfilepath <path>", "Specify the path to the project file to which weaving should be added", CommandOptionType.SingleValue);
                    var configurationType = c.Option("-c|--configurationtype <typefullname>", "The full name of the configuration type", CommandOptionType.SingleValue);
                    var assemblyExtension = c.Option("-a|--assemblyextension <extension>", "The extension of the assembly (when it has been built, e.g. 'dll', 'exe')", CommandOptionType.SingleValue);
                    var logLevel = c.Option("-l|--log <level>", "The logging level, one of verbose,debug,information,warning,error,fatal. Defaults to warning", CommandOptionType.SingleValue);

                    c.OnExecute(
                        () => {
                            LogConfiguration.Configure(logLevel);
                            if (!projectFilePath.HasValue()) {
                                Console.WriteLine("Please specify the path to the project file to which weaving should be added");
                                return 1;
                            }

                            if (!configurationType.HasValue()) {
                                Console.WriteLine("Please specify the configuration full name");
                                return 1;
                            }

                            if (!assemblyExtension.HasValue()) {
                                Console.WriteLine("Please specify the assembly extension e.g. 'dll', 'exe'");
                                return 1;
                            }

                            var projectFileFullPath = Path.GetFullPath(projectFilePath.Value());
                            AssemblySearchDirectories.Insert(0, Path.GetDirectoryName(projectFileFullPath)); // favour user code over dashing code
                            try {
                                ExecuteAddWeave(projectFileFullPath, configurationType, assemblyExtension);
                                return 0;
                            } catch (Exception ex) {
                                Console.WriteLine(ex.Message);
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteAddWeave(string projectFileFullPath, CommandOption configurationType, CommandOption assemblyExtension) {
            if (!File.Exists(projectFileFullPath)) {
                throw new Exception($"Project {projectFileFullPath} does not exist");
            }

            var xmlDocument = new XmlDocument();
            using (var projectFileStream = File.OpenRead(projectFileFullPath)) {
                xmlDocument.Load(projectFileStream);
            }
            
            if (!string.Equals("Project", xmlDocument.DocumentElement.LocalName, StringComparison.OrdinalIgnoreCase)) {
                throw new Exception("The project file must have a root element of <Project ...>");
            }

            var propertyGroup = xmlDocument.CreateElement("PropertyGroup");
            var property = xmlDocument.CreateElement("WeaveArguments");
            property.InnerText = $"-p \"$(MSBuildThisFileDirectory)$(OutputPath)$(AssemblyName).{assemblyExtension.Value()}\" -t \"{configurationType.Value()}\"";
            propertyGroup.AppendChild(property);
            var wasInserted = false;
            for (var i = 0; i < xmlDocument.DocumentElement.ChildNodes.Count; i++) {
                var childNode = xmlDocument.DocumentElement.ChildNodes[i];
                if (string.Equals("Import", childNode.LocalName, StringComparison.OrdinalIgnoreCase)
                    && (childNode.Attributes["Project"]?.Value?.ToUpperInvariant().Contains("DASHING") ?? false)) {
                    xmlDocument.DocumentElement.InsertBefore(propertyGroup, childNode);
                    wasInserted = true;
                    break;
                }
            }

            if (!wasInserted) {
                xmlDocument.DocumentElement.InsertAfter(propertyGroup, xmlDocument.DocumentElement.LastChild);
            }

            using (var writeProjectStream = File.OpenWrite(projectFileFullPath)) {
                xmlDocument.Save(writeProjectStream);
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetExtraPluralizationWords(CommandOption extraPluralizationWords) {
            return (extraPluralizationWords.Values ?? Enumerable.Empty<string>()).Select(
                s => {
                    var parts = s.Split('=');
                    if (parts.Length != 2) {
                        throw new Exception("Extra pluralization words must be in the format single=plural");
                    }

                    return new KeyValuePair<string, string>(parts[0], parts[1]);
                });
        }

        private static TInterfaceType LoadType<TInterfaceType>(string assemblyPath, string configurationFullName)
            where TInterfaceType : class {
            if (!File.Exists(assemblyPath)) {
                throw new Exception($"Unable to find assembly at {assemblyPath}");
            }

            if (!Path.IsPathRooted(assemblyPath)) {
                assemblyPath = Path.GetFullPath(assemblyPath);
            }

            var assembly = AssemblyContext.LoadFile(assemblyPath);
            var type = assembly.GetLoadableTypes()
                               .SingleOrDefault(t => t.FullName == configurationFullName);
            if (type == null) {
                throw new Exception($"Unable to find configuration of type {configurationFullName} in {assemblyPath}");
            }

            var instance = Activator.CreateInstance(type) as TInterfaceType;
            if (instance == null) {
                throw new Exception($"The type {configurationFullName} does not implement IConfiguration");
            }

            return instance;
        }

        private static void DisplayMigrationHeader(string assemblyPath, string configurationFullName, string connectionString) {
            using (Color(ConsoleColor.Yellow)) {
                Console.WriteLine("-- Dashing: Migration Script");
            }

            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", assemblyPath);
            Console.WriteLine("-- Class:    {0}", configurationFullName);
            Console.WriteLine("-- Connection:    {0}", connectionString);
            Console.WriteLine("-- ");

            using (Color(ConsoleColor.Yellow)) {
                Console.WriteLine("-- -------------------------------");
                Console.WriteLine("-- Migration is experimental:");
                Console.WriteLine("-- Please check the output!");
                Console.WriteLine("-- -------------------------------");
            }
        }

        private static ColorContext Color(ConsoleColor color) {
            return new ColorContext(color);
        }
    }
}