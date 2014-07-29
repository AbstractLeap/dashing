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
            if (!File.Exists(options.IniPath)) {
                throw new CatchyException("Could not locate configuration file {0}", options.IniPath);
            }

            // parse all of the configuration stuffs
            var config = IniParser.Parse(options.ProjectName + ".ini");

            var connectionString = new ConnectionStringSettings();
            connectionString = IniParser.AssignTo(config["Database"], connectionString);

            var dashingSettings = new DashingSettings();
            dashingSettings = IniParser.AssignTo(config["Dashing"], dashingSettings);

            var reverseEngineerSettings = new ReverseEngineerSettings();
            reverseEngineerSettings = IniParser.AssignTo(config["ReverseEngineer"], reverseEngineerSettings);

            if (!File.Exists(dashingSettings.PathToDll)) {
                throw new CatchyException("Could not locate {0}", dashingSettings.PathToDll);
            }

            if (options.Script) {
                GenerateMigrationScript(
                    options.Location,
                    options.Naive,
                    connectionString,
                    dashingSettings);
                return;
            }

            if (options.Migration) {
                PerformMigration(options.Naive, connectionString, dashingSettings);
                return;
            }

            if (options.ReverseEngineer) {
                // overwrite the path with the default if necessary
                if (string.IsNullOrEmpty(options.Location)) {
                    options.Location = dashingSettings.DefaultSavePath;
                }

                // if it is still empty, ...
                if (string.IsNullOrEmpty(options.Location) && options.ReverseEngineer) {
                    throw new CatchyException("You must specify a location for generated files to be saved");
                    ////if (Directory.Exists(path)) {
                    ////path = string.Format("Migration-{0}-{1:yyyy-MM-dd-HH-mm-ss}.sql", Settings.Default.ConfigurationTypeName.Replace('.', '_'), DateTime.Now);
                    ////}
                }

                if (string.IsNullOrEmpty(reverseEngineerSettings.GeneratedNamespace)) {
                    throw new CatchyException("You must specify a GeneratedNamespace in the Project ini file");
                }

                ReverseEngineer(
                    options.Location,
                    reverseEngineerSettings.GeneratedNamespace,
                    connectionString);
            }
        }

        private static void ShowHelpText(CommandLineOptions options) {
            Console.Write(HelpText.AutoBuild(options));
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

                    using (new TimedOperation("-- Executing migration script on {0}", connection.ConnectionString))
                    using (var command = connection.CreateCommand()) {
                        command.CommandText = script;
                        command.ExecuteNonQuery();
                    }

                    // now let's call Seed
                    var config = GetConfig(dashingSettings);
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


        private static void GenerateMigrationScript(
            string pathOrNull,
            bool naive,
            ConnectionStringSettings connectionStringSettings,
            DashingSettings dashingSettings) {
            if (!naive) {
                NotImplemented();
            }

            var migrationScript = GenerateNaiveMigrationScript(connectionStringSettings, dashingSettings);
            using (var writer = string.IsNullOrEmpty(pathOrNull) ? Console.Out : new StreamWriter(File.OpenWrite(pathOrNull))) {
                writer.WriteLine(migrationScript);
            }
        }

        private static string GenerateNaiveMigrationScript(
            ConnectionStringSettings connectionStringSettings,
            DashingSettings dashingSettings) {
            Console.WriteLine();
            using (Color(ConsoleColor.Yellow))
                Console.WriteLine("-- Dashing: Naive Migration Script");
            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", dashingSettings.PathToDll);
            Console.WriteLine("-- Class:    {0}", dashingSettings.ConfigurationName);
            Console.WriteLine();
            
            // fetch the to state
            IConfiguration config;
            using (new TimedOperation("-- Fetching configuration contents...")) {
                config = GetConfig(dashingSettings);
            }

            // fetch the from state
            IEnumerable<IMap> maps;
            using (new TimedOperation("-- Reading database contents...")) {
                DatabaseSchema schema;
                maps = ReverseEngineerMaps(out schema, connectionStringSettings);
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

        private static IConfiguration GetConfig(DashingSettings dashingSettings) {
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