namespace Dashing.Cli.Commands {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.CommandLine;

    using McMaster.Extensions.CommandLineUtils;

    public static class ScriptExtensions {
        public static IEnumerable<KeyValuePair<string, string>> GetExtraPluralizationWords(this CommandOption extraPluralizationWords) {
            return (extraPluralizationWords.Values ?? Enumerable.Empty<string>()).Select(
                s => {
                    var parts = s.Split('=');
                    if (parts.Length != 2) {
                        throw new Exception("Extra pluralization words must be in the format single=plural");
                    }

                    return new KeyValuePair<string, string>(parts[0], parts[1]);
                });
        }

        public static void DisplayMigrationHeader(string assemblyPath, string configurationFullName, string connectionString) {
            using (new ColorContext(ConsoleColor.Yellow)) {
                Console.WriteLine("-- Dashing: Migration Script");
            }

            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", assemblyPath);
            Console.WriteLine("-- Class:    {0}", configurationFullName);
            Console.WriteLine("-- Connection:    {0}", connectionString);
            Console.WriteLine("-- ");

            using (new ColorContext(ConsoleColor.Yellow)) {
                Console.WriteLine("-- -------------------------------");
                Console.WriteLine("-- Migration is experimental:");
                Console.WriteLine("-- Please check the output!");
                Console.WriteLine("-- -------------------------------");
            }
        }
    }
}