namespace Dashing.Console {
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using CommandLine;
    using CommandLine.Text;

    using Dashing.Configuration;
    using Dashing.Console.Settings;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools;
    using Dashing.Tools.Migration;
    using Dashing.Tools.ModelGeneration;
    using Dashing.Tools.ReverseEngineering;

    using DatabaseSchemaReader;
    using DatabaseSchemaReader.DataSchema;

    using SharpConfig;

    internal class Program {
        private static void Main(string[] args) {
            var options = new CommandLineOptions();
            bool showHelp = false;
            if (Parser.Default.ParseArguments(args, options)) {
                // find the project and read the file
                if (!File.Exists(options.ProjectName + ".ini")) {
                    Console.WriteLine("Ensure the project name matches the ini file name");
                    showHelp = true;
                }
                else {
                    var connectionString = new ConnectionStringSettings();
                    var dashingSettings = new DashingSettings();
                    var reverseEngineerSettings = new ReverseEngineerSettings();
                    //var config = Configuration.Load(options.ProjectName + ".ini", ParseFlags.IgnoreComments);
                    //connectionString = config["Database"].AssignTo(connectionString);
                    //dashingSettings = config["Dashing"].AssignTo(dashingSettings);
                    //reverseEngineerSettings =
                    //    config["ReverseEngineer"].AssignTo(reverseEngineerSettings);
                    var config = IniParser.Parse(options.ProjectName + ".ini");
                    connectionString = IniParser.AssignTo(config["Database"], connectionString);
                    dashingSettings = IniParser.AssignTo(config["Dashing"], dashingSettings);
                    reverseEngineerSettings = IniParser.AssignTo(
                        config["ReverseEngineer"],
                        reverseEngineerSettings);


                    // overwrite the path with the default if necessary
                    if (string.IsNullOrEmpty(options.Location)) {
                        options.Location = dashingSettings.DefaultSavePath;
                        if (string.IsNullOrEmpty(options.Location)
                            && (options.Script || options.ReverseEngineer)) {
                            Console.WriteLine(
                                "You must specify a location for generated files to be saved");
                            showHelp = true;
                        }
                    }

                    if (!showHelp) {
                        if (options.Script) {
                            GenerateMigrationScript(
                                options.Location,
                                options.Naive,
                                connectionString,
                                dashingSettings);
                        }
                        else if (options.Migration) {
                            PerformMigration(options.Naive, connectionString, dashingSettings);
                        }
                        else if (options.ReverseEngineer) {
                            if (string.IsNullOrEmpty(reverseEngineerSettings.GeneratedNamespace)) {
                                Console.WriteLine(
                                    "You must specify a GeneratedNamespace in the Project ini file");
                                showHelp = true;
                            }

                            if (!showHelp) {
                                ReverseEngineer(
                                    options.Location,
                                    reverseEngineerSettings.GeneratedNamespace,
                                    connectionString);
                            }
                        }
                        else {
                            showHelp = true;
                        }
                    }
                }
            }
            else {
                showHelp = true;
            }

            if (showHelp) {
                Console.Write(HelpText.AutoBuild(options));
            }
        }

        private static void ReverseEngineer(
            string path,
            string generatedNamespace,
            ConnectionStringSettings connectionStringSettings) {
            DatabaseSchema schema;
            var maps = ReverseEngineerMaps(out schema, connectionStringSettings);
            var reverseEngineer = new ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, generatedNamespace);

            foreach (var source in sources) {
                File.WriteAllText(path + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static IEnumerable<IMap> ReverseEngineerMaps(
            out DatabaseSchema schema,
            ConnectionStringSettings connectionStringSettings) {
            var engineer = new Engineer();
            var databaseReader = new DatabaseReader(
                connectionStringSettings.ConnectionString,
                connectionStringSettings.ProviderName);
            schema = databaseReader.ReadAll();
            return engineer.ReverseEngineer(schema);
        }

        private static void PerformMigration(
            bool naive,
            ConnectionStringSettings connectionStringSettings,
            DashingSettings dashingSettings) {
            if (naive) {
                var script = GenerateNaiveMigrationScript(connectionStringSettings, dashingSettings);
                var factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);

                using (var connection = factory.CreateConnection()) {
                    if (connection == null) {
                        throw new Exception("Could not connect to database");
                    }

                    connection.ConnectionString = connectionStringSettings.ConnectionString;
                    connection.Open();

                    using (var command = connection.CreateCommand()) {
                        command.CommandText = script;
                        command.ExecuteNonQuery();
                    }

                    // now let's call Seed
                    var config = GetConfig(dashingSettings);
                    var seederConfig = config as ISeeder;
                    if (seederConfig != null) {
                        Console.WriteLine("Seeding Database...");
                        using (var session = config.BeginSession(connection)) {
                            seederConfig.Seed(session);
                            session.Complete();
                        }
                    }
                }

                return;
            }

            NotImplemented();
        }

        private static void GenerateMigrationScript(
            string path,
            bool naive,
            ConnectionStringSettings conenctionStringSettings,
            DashingSettings dashingSettings) {
            if (naive) {
                var script = GenerateNaiveMigrationScript(conenctionStringSettings, dashingSettings);
                File.WriteAllText(path, script);
                return;
            }

            NotImplemented();
        }

        private static string GenerateNaiveMigrationScript(
            ConnectionStringSettings connectionStringSettings,
            DashingSettings dashingSettings) {
            // fetch the from state
            DatabaseSchema schema;
            var maps = ReverseEngineerMaps(out schema, connectionStringSettings);

            var config = GetConfig(dashingSettings);
            var dialectFactory = new DialectFactory();
            var dialect =
                dialectFactory.Create(
                    new System.Configuration.ConnectionStringSettings(
                        "Default",
                        connectionStringSettings.ConnectionString,
                        connectionStringSettings.ProviderName));
            var migrator = new Migrator(
                new CreateTableWriter(dialect),
                new DropTableWriter(dialect),
                null);
            IEnumerable<string> warnings, errors;
            var script = migrator.GenerateNaiveSqlDiff(maps, config.Maps, out warnings, out errors);
            return script;
        }

        private static IConfiguration GetConfig(DashingSettings dashingSettings) {
            // fetch the to state
            var configAssembly = Assembly.LoadFrom(dashingSettings.PathToDll);
            var configType =
                configAssembly.DefinedTypes.First(
                    t => t.FullName == dashingSettings.ConfigurationName);

            // TODO add in a factory way of generating the config for cases where constructor not empty
            var config = (IConfiguration)Activator.CreateInstance(configType);
            return config;
        }

        private static string GetPath(string[] args, string option) {
            var indexOf = Array.IndexOf(args, option);
            if (indexOf == args.Length - 1) {
                Help();
                return string.Empty;
            }

            var path = args[indexOf + 1];
            return path;
        }

        private static string GetNamespace(string[] args, string option) {
            var indexOf = Array.IndexOf(args, option);
            if (indexOf == args.Length - 2) {
                Help();
                return string.Empty;
            }

            var nspace = args[indexOf + 2];
            return nspace;
        }

        private static void Help() {
            Console.WriteLine("DB Manager Help");
            Console.WriteLine("------------------------");
            Console.WriteLine("Usage: dbmanager [options]");
            Console.WriteLine();
            Console.WriteLine("Options: ");
            Console.WriteLine(
                "-s <path> [-n] : Generate a migration script and output to path. Optionally specify -n to generate a naive migration");
            Console.WriteLine(
                "-m [-n] : Run a migration on the db specified in the settings. Optionally specify -n to generate a naive migration");
            Console.WriteLine(
                "-r <path> <namespace> : Reverse engineer a db in to a map and save at the specified path");
        }

        private static void NotImplemented() {
            Console.WriteLine("Sorry, that's not implemented yet.");
            Environment.Exit(1);
        }
    }
}