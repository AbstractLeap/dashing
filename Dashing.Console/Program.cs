namespace Dashing.Console {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Dashing.Configuration;
    using Dashing.Console.Properties;
    using Dashing.Engine;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.ModelGeneration;
    using Dashing.Tools.ReverseEngineering;

    using DatabaseSchemaReader;
    using DatabaseSchemaReader.DataSchema;

    internal class Program {
        private static void Main(string[] args) {
            if (!File.Exists(Settings.Default.ConfigurationDllPath)) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine("Path {0} not found!", new FileInfo(Settings.Default.ConfigurationDllPath).FullName);
                    return;
                }
            }

            Console.WriteLine();
            using (Color(ConsoleColor.Yellow))
                Console.WriteLine("-- Dashing DbManager");
            Console.WriteLine("-- -----------------");
            Console.WriteLine("-- Assembly: {0}", Settings.Default.ConfigurationDllPath);
            Console.WriteLine("-- Class:    {0}", Settings.Default.ConfigurationTypeName);
            Console.WriteLine();

            if (args.Contains("-s")) {
                var path = GetPath(args, "-s");
                if (Directory.Exists(path)) {
                    path = string.Format("Migration-{0}-{1:yyyy-MM-dd-HH-mm-ss}.sql", Settings.Default.ConfigurationTypeName.Replace('.', '_'), DateTime.Now);
                }

                using (var writer = string.IsNullOrEmpty(path) ? Console.Out : new StreamWriter(File.OpenWrite(path)))
                    GenerateMigrationScript(writer, args.Contains("-n"));

                return;
            }

            if (args.Contains("-m")) {
                PerformMigration(args.Contains("-n"));
                return;
            }

            if (args.Contains("-r")) {
                string path = GetPath(args, "-r");
                string generatedNamespace = GetNamespace(args, "-r");
                if (path != string.Empty && generatedNamespace != string.Empty) {
                    ReverseEngineer(path, generatedNamespace);
                }

                return;
            }

            Help();
        }

        private static void ReverseEngineer(string path, string generatedNamespace) {
            DatabaseSchema schema;
            ConnectionStringSettings connectionString;
            var maps = ReverseEngineerMaps(out schema, out connectionString);
            var reverseEngineer = new ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, generatedNamespace);

            foreach (var source in sources) {
                File.WriteAllText(path + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static IEnumerable<IMap> ReverseEngineerMaps(out DatabaseSchema schema, out ConnectionStringSettings connectionString) {
            var engineer = new Engineer();
            connectionString = ConfigurationManager.ConnectionStrings["Default"];
            var databaseReader = new DatabaseReader(connectionString.ConnectionString, connectionString.ProviderName);
            schema = databaseReader.ReadAll();
            return engineer.ReverseEngineer(schema);
        }

        private static void PerformMigration(bool naive) {
            if (naive) {
                ConnectionStringSettings connectionString;
                var script = GenerateNaiveMigrationScript(out connectionString);
                var factory = DbProviderFactories.GetFactory(connectionString.ProviderName);

                using (var connection = factory.CreateConnection()) {
                    if (connection == null) {
                        throw new Exception("Could not connect to database");
                    }

                    connection.ConnectionString = connectionString.ConnectionString;
                    connection.Open();

                    using (new TimedOperation("Executing migration script"))
                    using (var command = connection.CreateCommand()) {
                        command.CommandText = script;
                        command.ExecuteNonQuery();
                    }
                }

                return;
            }

            NotImplemented();
        }

        private static void GenerateMigrationScript(TextWriter writer, bool naive) {
            if (naive) {
                ConnectionStringSettings connectionString;
                writer.WriteLine(GenerateNaiveMigrationScript(out connectionString));
                return;
            }

            NotImplemented();
        }

        private static string GenerateNaiveMigrationScript(out ConnectionStringSettings connectionString) {
            // fetch the from state
            IEnumerable<IMap> maps;
            using (new TimedOperation("--  Reading database contents...")) {
                DatabaseSchema schema;
                maps = ReverseEngineerMaps(out schema, out connectionString);
            }

            // fetch the to state
            TypeInfo configType;
            using (new TimedOperation("--- Fetching configuration contents...")) {
                var configAssembly = Assembly.LoadFrom(Settings.Default.ConfigurationDllPath);
                configType = configAssembly.DefinedTypes.SingleOrDefault(t => t.FullName == Settings.Default.ConfigurationTypeName);

                if (configType == null) {
                    using (Color(ConsoleColor.Red)) {
                        var candidates = configAssembly.DefinedTypes.Where(t => t.GetInterface(typeof(IConfiguration).FullName) != null).ToArray();
                        if (candidates.Any()) {
                            Console.WriteLine("Could not locate {0}, but found candidates: {1}", Settings.Default.ConfigurationTypeName, string.Join(", ", candidates.Select(c => c.FullName)));
                            return string.Empty;
                        }

                        Console.WriteLine("Could not locate {0}, and found no candidate configurations", Settings.Default.ConfigurationTypeName);
                        return string.Empty;
                    }
                }
            }


            // TODO add in a factory way of generating the config for cases where constructor not empty
            var config = (IConfiguration)Activator.CreateInstance(configType);
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(connectionString);
            var migrator = new Migrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), null);
            IEnumerable<string> warnings, errors;
            var script = migrator.GenerateNaiveSqlDiff(maps, config.Maps, out warnings, out errors);
            return script;
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
            Console.WriteLine("-s <path> [-n] : Generate a migration script and output to path. Optionally specify -n to generate a naive migration");
            Console.WriteLine("-m [-n] : Run a migration on the db specified in the settings. Optionally specify -n to generate a naive migration");
            Console.WriteLine("-r <path> <namespace> : Reverse engineer a db in to a map and save at the specified path");
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