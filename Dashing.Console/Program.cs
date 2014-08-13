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

    internal class Program {
        private static void Main(string[] args) {
            try {
                InnerMain(args);
            }
            catch (CatchyException e) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private class CatchyException : Exception {
            public CatchyException(string message)
                : base(message) { }

            public CatchyException(string format, params object[] args)
                : base(string.Format(format, args)) { }
        }

        private static void InnerMain(string[] args) {
            var options = new CommandLineOptions();

            if (!Parser.Default.ParseArguments(args, options)) {
                ShowHelpText(options);
                return;
            }

            // prevalidation
            if (string.IsNullOrWhiteSpace(options.ConfigPath)) {
                throw new CatchyException("You must specify a configuration path or a project name");
            }

            if (!File.Exists(options.ConfigPath)) {
                throw new CatchyException("Could not locate configuration file {0}", options.ConfigPath);
            }

            // parse all of the configuration stuffs
            ConnectionStringSettings connectionStringSettings;
            DashingSettings dashingSettings;
            ReverseEngineerSettings reverseEngineerSettings;
            ParseConfiguration(options, out connectionStringSettings, out dashingSettings, out reverseEngineerSettings);

            // postvalidation
            if (!File.Exists(dashingSettings.PathToDll)) {
                throw new CatchyException("Could not locate {0}", dashingSettings.PathToDll);
            }

            // now decide what to do
            if (options.Script) {
                DoScript(options.Location, options.Naive, connectionStringSettings, dashingSettings);
            }
            else if (options.Migration) {
                DoMigrate(options.Naive, connectionStringSettings, dashingSettings);
            } 
            else if (options.ReverseEngineer) {
                DoReverseEngineer(options, dashingSettings, reverseEngineerSettings, connectionStringSettings);
            }
            else {
                ShowHelpText(options);
            }
        }

        private static void ParseConfiguration(CommandLineOptions options, out ConnectionStringSettings connectionStringSettings, out DashingSettings dashingSettings, out ReverseEngineerSettings reverseEngineerSettings) {
            var config = IniParser.Parse(options.ProjectName + ".ini");

            connectionStringSettings = new ConnectionStringSettings();
            connectionStringSettings = IniParser.AssignTo(config["Database"], connectionStringSettings);

            dashingSettings = new DashingSettings();
            dashingSettings = IniParser.AssignTo(config["Dashing"], dashingSettings);

            reverseEngineerSettings = new ReverseEngineerSettings();
            reverseEngineerSettings = IniParser.AssignTo(config["ReverseEngineer"], reverseEngineerSettings);
        }

        private static void DoScript(
            string pathOrNull,
            bool naive,
            ConnectionStringSettings connectionStringSettings,
            DashingSettings dashingSettings) {
            if (!naive) {
                NotImplemented();
            }

            Console.WriteLine();
            using (Color(ConsoleColor.Yellow))
                Console.WriteLine("-- Dashing: Naive Migration Script");
            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", dashingSettings.PathToDll);
            Console.WriteLine("-- Class:    {0}", dashingSettings.ConfigurationName);
            Console.WriteLine();

            var migrationScript = GenerateNaiveMigrationScript(connectionStringSettings, dashingSettings);
            using (var writer = string.IsNullOrEmpty(pathOrNull) ? Console.Out : new StreamWriter(File.OpenWrite(pathOrNull))) {
                writer.WriteLine(migrationScript);
            }
        }

        private static void DoMigrate(
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

                    using (new TimedOperation("-- Executing migration script on {0}", connection.ConnectionString))
                    using (var command = connection.CreateCommand()) {
                        command.CommandText = script;
                        command.ExecuteNonQuery();
                    }

                    // now let's call Seed
                    var config = LoadConfiguration(dashingSettings);
                    var seederConfig = config as ISeeder;
                    if (seederConfig != null) {
                        using (new TimedOperation("-- Executing seeds"))
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

        private static void DoReverseEngineer(CommandLineOptions options, DashingSettings dashingSettings, ReverseEngineerSettings reverseEngineerSettings, ConnectionStringSettings connectionString) {
            // overwrite the path with the default if necessary
            if (string.IsNullOrEmpty(options.Location)) {
                options.Location = dashingSettings.DefaultSavePath;
            }

            // if it is still empty, ...
            if (string.IsNullOrEmpty(options.Location) && options.ReverseEngineer) {
                throw new CatchyException("You must specify a location for generated files to be saved");
            }

            // require a generated namespace
            if (string.IsNullOrEmpty(reverseEngineerSettings.GeneratedNamespace)) {
                throw new CatchyException("You must specify a GeneratedNamespace in the Project ini file");
            }

            DatabaseSchema schema;
            var engineer = new Engineer();
            var databaseReader = new DatabaseReader(
                connectionString.ConnectionString,
                connectionString.ProviderName);
            schema = databaseReader.ReadAll();
            var maps = engineer.ReverseEngineer(schema);
            var reverseEngineer = new ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, reverseEngineerSettings.GeneratedNamespace);

            foreach (var source in sources) {
                File.WriteAllText(options.Location + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static void ShowHelpText(CommandLineOptions options) {
            Console.Write(HelpText.AutoBuild(options));
        }

        private static string GenerateNaiveMigrationScript(
            ConnectionStringSettings connectionStringSettings,
            DashingSettings dashingSettings) {
            // fetch the to state
            IConfiguration config;
            using (new TimedOperation("-- Fetching configuration contents...")) {
                config = LoadConfiguration(dashingSettings);
            }

            // fetch the from state
            IEnumerable<IMap> maps;
            using (new TimedOperation("-- Reading database contents...")) {
                DatabaseSchema schema;
                var engineer = new Engineer();
                var databaseReader = new DatabaseReader(
                    connectionStringSettings.ConnectionString,
                    connectionStringSettings.ProviderName);
                schema = databaseReader.ReadAll();
                maps = engineer.ReverseEngineer(schema);
            }

            // set up migrator
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(connectionStringSettings.ToSystem());
            var migrator = new Migrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), null);
            
            // run the migrator
            IEnumerable<string> warnings, errors;
            var script = migrator.GenerateNaiveSqlDiff(maps, config.Maps, out warnings, out errors);

            // TODO: do things with warnings and errors
            return script;
        }

        private static IConfiguration LoadConfiguration(DashingSettings dashingSettings) {
            // fetch the to state
            var configAssembly = Assembly.LoadFrom(dashingSettings.PathToDll);
            var configType = configAssembly.DefinedTypes.SingleOrDefault(t => t.FullName == dashingSettings.ConfigurationName);

            if (configType == null) {
                using (Color(ConsoleColor.Red)) {
                    var candidates = configAssembly.DefinedTypes.Where(t => t.GetInterface(typeof(IConfiguration).FullName) != null).ToArray();
                    if (candidates.Any()) {
                        throw new CatchyException("Could not locate {0}, but found candidates: {1}", dashingSettings.ConfigurationName, string.Join(", ", candidates.Select(c => c.FullName)));
                    }

                    throw new CatchyException("Could not locate {0}, and found no candidate configurations", dashingSettings.ConfigurationName);
                }
            }

            // TODO add in a factory way of generating the config for cases where constructor not empty
            var config = (IConfiguration)Activator.CreateInstance(configType);
            return config;
        }

        private static void NotImplemented() {
            Console.WriteLine("Sorry, that's not implemented yet.");
            Environment.Exit(1);
        }

        private static ColorContext Color(ConsoleColor color) {
            return new ColorContext(color);
        }
    }
}