using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace dbmanager
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("-s"))
            {
                string path = GetPath(args, "-s");
                if (path != string.Empty)
                {
                    GenerateMigrationScript(path, args.Contains("-n"));
                }

                return;
            }

            if (args.Contains("-m"))
            {
                PerformMigration(args.Contains("-n"));
                return;
            }

            if (args.Contains("-r"))
            {
                string path = GetPath(args, "-r");
                string generatedNamespace = GetNamespace(args, "-r");
                if (path != string.Empty && generatedNamespace != string.Empty)
                {
                    ReverseEngineer(path, generatedNamespace);
                }               

                return;
            }

            Help();
        }

        private static void ReverseEngineer(string path, string generatedNamespace)
        {
            DatabaseSchemaReader.DataSchema.DatabaseSchema schema;
            System.Configuration.ConnectionStringSettings connectionString;
            var maps = ReverseEngineerMaps(out schema, out connectionString);
            var reverseEngineer = new TopHat.Tools.ModelGeneration.ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, generatedNamespace);

            foreach (var source in sources)
            {
                System.IO.File.WriteAllText(path + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static IEnumerable<TopHat.Configuration.IMap> ReverseEngineerMaps(out DatabaseSchemaReader.DataSchema.DatabaseSchema schema, out System.Configuration.ConnectionStringSettings connectionString)
        {
            var engineer = new TopHat.Tools.ReverseEngineering.Engineer();
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Default"];
            var databaseReader = new DatabaseSchemaReader.DatabaseReader(connectionString.ConnectionString, connectionString.ProviderName);
            schema = databaseReader.ReadAll();
            return engineer.ReverseEngineer(schema);
        }

        private static void PerformMigration(bool naive)
        {
            if (naive)
            {
                System.Configuration.ConnectionStringSettings connectionString;
                var script = GenerateNaiveMigrationScript(out connectionString);
                var factory = System.Data.Common.DbProviderFactories.GetFactory(connectionString.ProviderName);
                var connection = factory.CreateConnection();
                connection.ConnectionString = connectionString.ConnectionString;
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = script;
                command.ExecuteNonQuery();
                command.Dispose();
                connection.Dispose();
                return;
            }

            NotImplemented();
        }

        private static void GenerateMigrationScript(string path, bool naive)
        {
            if (naive)
            {
                System.Configuration.ConnectionStringSettings connectionString;
                var script = GenerateNaiveMigrationScript(out connectionString);
                System.IO.File.WriteAllText(path, script);
                return;
            }

            NotImplemented();
        }

        private static string GenerateNaiveMigrationScript(out System.Configuration.ConnectionStringSettings connectionString)
        {
            // fetch the from state
            DatabaseSchemaReader.DataSchema.DatabaseSchema schema;
            var maps = ReverseEngineerMaps(out schema, out connectionString);

            // fetch the to state
            var configAssembly = Assembly.LoadFrom(Properties.Settings.Default.ConfigurationDllPath);
            var configType = configAssembly.DefinedTypes.First(t => t.FullName == Properties.Settings.Default.ConfigurationTypeName);

            // TODO add in a factory way of generating the config for cases where constructor not empty
            var config = Activator.CreateInstance(configType) as TopHat.Configuration.IConfiguration;
            var dialectFactory = new TopHat.Engine.DialectFactory();
            var dialect = dialectFactory.Create(connectionString);
            var migrator = new TopHat.Tools.Migration.Migrator(new TopHat.Engine.DDL.CreateTableWriter(dialect), new TopHat.Engine.DDL.DropTableWriter(dialect), null);
            IEnumerable<string> warnings, errors;
            var script = migrator.GenerateNaiveSqlDiff(maps, config.Maps, out warnings, out errors);
            return script;
        }

        private static string GetPath(string[] args, string option)
        {
            int sIdx = Array.IndexOf(args, option);
            if (sIdx == args.Length - 1)
            {
                Help();
                return string.Empty;
            }

            string path = args[sIdx + 1];
            return path;
        }

        private static string GetNamespace(string[] args, string option)
        {
            int sIdx = Array.IndexOf(args, option);
            if (sIdx == args.Length - 2)
            {
                Help();
                return string.Empty;
            }

            string nspace = args[sIdx + 2];
            return nspace;
        }

        private static void Help()
        {
            Console.WriteLine("DB Manager Help");
            Console.WriteLine("------------------------");
            Console.WriteLine("Usage: dbmanager [options]");
            Console.WriteLine();
            Console.WriteLine("Options: ");
            Console.WriteLine("-s <path> [-n] : Generate a migration script and output to path. Optionally specify -n to generate a naive migration");
            Console.WriteLine("-m [-n] : Run a migration on the db specified in the settings. Optionally specify -n to generate a naive migration");
            Console.WriteLine("-r <path> <namespace> : Reverse engineer a db in to a map and save at the specified path");
        }

        private static void NotImplemented()
        {
            Console.WriteLine("Sorry, that's not implemented yet.");
            Environment.Exit(1);
        }
    }
}
