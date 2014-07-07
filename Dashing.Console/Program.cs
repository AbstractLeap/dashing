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
            if (args.Contains("-s")) {
                string path = GetPath(args, "-s");
                if (path != string.Empty) {
                    GenerateMigrationScript(path, args.Contains("-n"));
                }

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

                    using (var command = connection.CreateCommand()) {
                        command.CommandText = script;
                        command.ExecuteNonQuery();
                    }
                }

                return;
            }

            NotImplemented();
        }

        private static void GenerateMigrationScript(string path, bool naive) {
            if (naive) {
                ConnectionStringSettings connectionString;
                var script = GenerateNaiveMigrationScript(out connectionString);
                File.WriteAllText(path, script);
                return;
            }

            NotImplemented();
        }

        private static string GenerateNaiveMigrationScript(out ConnectionStringSettings connectionString) {
            // fetch the from state
            DatabaseSchema schema;
            var maps = ReverseEngineerMaps(out schema, out connectionString);

            // fetch the to state
            var configAssembly = Assembly.LoadFrom(Settings.Default.ConfigurationDllPath);
            var configType = configAssembly.DefinedTypes.First(t => t.FullName == Settings.Default.ConfigurationTypeName);

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
    }
}