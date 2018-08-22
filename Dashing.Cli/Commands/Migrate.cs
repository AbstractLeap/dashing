namespace Dashing.Cli.Commands {
    using System;
    using System.IO;

    using Dashing.CommandLine;
    using Dashing.Configuration;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;

    public static class Migrate {
        public static void UseMigrate(this CommandLineApplication app) {
            app.Command(
                "migrate",
                c => {
                    c.Description = "Generates and runs a script that will migrate an existing database so that it matches the specified configuration";

                    // attempts to weave the assemblies at the specified location
                    var assemblyPath = c.Option("-a|--assemblypath <path>", "Specify the path to the assembly that contains the configuration", CommandOptionType.SingleValue)
                                        .IsRequired();
                    var configurationType = c.Option("-t|--typefullname <typefullname>", "The full name of the configuration type that describes the domain", CommandOptionType.SingleValue)
                                             .IsRequired();
                    var connectionString = c.Option("-c|--connection <connectionstring>", "The connection string of the database that you would like to migrate", CommandOptionType.SingleValue)
                                            .IsRequired();
                    var provider = c.Option("-p|--provider <providername>", "The provider name for the database that you are migrating", CommandOptionType.SingleValue);
                    var tablesToIgnore = c.Option("-ti|--tablestoignore <tablename>", "The name of any tables which should be ignored in the migration", CommandOptionType.MultipleValue);
                    var indexesToIgnore = c.Option("-ii|--indexestoignore <indexname>", "The name of any indexes which should be ignored in the migration", CommandOptionType.MultipleValue);
                    var extraPluralizationWords = c.Option("-ep|--extraplurals <singlename,pluralname>", "Any extra single/plural pairs that need adding", CommandOptionType.MultipleValue);

                    c.OnExecute(
                        () => {
                            var assemblyDir = Path.GetDirectoryName(assemblyPath.Value());
                            Program.AssemblySearchDirectories.Insert(0, assemblyDir); // favour user code over dashing code
                            ScriptExtensions.DisplayMigrationHeader(assemblyPath.Value(), configurationType.Value(), connectionString.Value());
                            try {
                                ExecuteMigrate(assemblyPath, configurationType, connectionString, provider, tablesToIgnore, indexesToIgnore, extraPluralizationWords);
                                return 0;
                            }
                            catch (Exception ex) {
                                Log.Logger.Fatal(ex, "migrate failed unexpectedly");
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteMigrate(CommandOption assemblyPath, CommandOption configurationType, CommandOption connectionString, CommandOption provider, CommandOption tablesToIgnore, CommandOption indexesToIgnore, CommandOption extraPluralizationWords) {
            var databaseMigrator = new DatabaseMigrator();
            databaseMigrator.Execute(
                AssemblyContext.LoadType<IConfiguration>(assemblyPath.Value(), configurationType.Value()),
                connectionString.Value(),
                provider.HasValue()
                    ? provider.Value()
                    : "System.Data.SqlClient",
                tablesToIgnore.Values,
                indexesToIgnore.Values,
                extraPluralizationWords.GetExtraPluralizationWords(),
                new ConsoleAnswerProvider());
        }
    }
}