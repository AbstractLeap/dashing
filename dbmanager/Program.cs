using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                    GenerateMigrationScripts(path);
                }

                return;
            }

            if (args.Contains("-m"))
            {
                PerformMigration();
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
            var engineer = new TopHat.Tools.ReverseEngineering.Engineer();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Default"];
            var databaseReader = new DatabaseSchemaReader.DatabaseReader(connectionString.ConnectionString, connectionString.ProviderName);
            var schema = databaseReader.ReadAll();
            var maps = engineer.ReverseEngineer(schema);
            var reverseEngineer = new TopHat.Tools.ModelGeneration.ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, generatedNamespace);

            foreach (var source in sources)
            {
                System.IO.File.WriteAllText(path + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static void PerformMigration()
        {
            throw new NotImplementedException();
        }

        private static void GenerateMigrationScripts(string path)
        {
            throw new NotImplementedException();
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
            Console.WriteLine("-s <path> : Generate a migration script and output to path");
            Console.WriteLine("-m : Run a migration on the db specified in the settings");
            Console.WriteLine("-r <path> <namespace> : Reverse engineer a db in to a map and save at the specified path");
        }
    }
}
