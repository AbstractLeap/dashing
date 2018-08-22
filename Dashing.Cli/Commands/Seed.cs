namespace Dashing.Cli.Commands {
    using System;
    using System.IO;

    using Dashing.CommandLine;
    using Dashing.Configuration;

    using McMaster.Extensions.CommandLineUtils;

    using Serilog;

    public static class Seed {
        public static void UseSeed(this CommandLineApplication app) {
            app.Command(
                "seed",
                c => {
                    c.Description = "Executes a function to seed a database";

                    // attempts to weave the assemblies at the specified location
                    var configurationAssemblyPath = c.Option("-ca|--configurationassemblypath <path>", "Specify the path to the assembly that contains the configuration", CommandOptionType.SingleValue)
                                                     .IsRequired();
                    var configurationType = c.Option("-ct|--typefullname <typefullname>", "The full name of the configuration type", CommandOptionType.SingleValue)
                                             .IsRequired();
                    var seederAssemblyPath = c.Option("-sa|--configurationassemblypath <path>", "Specify the path to the assembly that contains the seeder", CommandOptionType.SingleValue)
                                              .IsRequired();
                    var seederType = c.Option("-st|--typefullname <typefullname>", "The full name of the seeder type", CommandOptionType.SingleValue)
                                      .IsRequired();
                    var connectionString = c.Option("-c|--connection <connectionstring>", "The connection string of the database that you would like to migrate", CommandOptionType.SingleValue)
                                            .IsRequired();
                    var provider = c.Option("-p|--provider <providername>", "The provider name for the database that you are migrating", CommandOptionType.SingleValue);

                    c.OnExecute(
                        () => {
                            var assemblyDir = Path.GetDirectoryName(configurationAssemblyPath.Value());
                            Program.AssemblySearchDirectories.Insert(0, assemblyDir); // favour user code over dashing code
                            try {
                                ExecuteSeed(seederAssemblyPath, seederType, configurationAssemblyPath, configurationType, connectionString, provider);
                                return 0;
                            }
                            catch (Exception ex) {
                                Log.Logger.Fatal(ex, "seed failed unexpectedly");
                                return 1;
                            }
                        });
                });
        }

        private static void ExecuteSeed(CommandOption seederAssemblyPath, CommandOption seederType, CommandOption configurationAssemblyPath, CommandOption configurationType, CommandOption connectionString, CommandOption provider) {
            var seeder = new Seeder();
            seeder.Execute(
                AssemblyContext.LoadType<ISeeder>(seederAssemblyPath.Value(), seederType.Value()),
                AssemblyContext.LoadType<IConfiguration>(configurationAssemblyPath.Value(), configurationType.Value()),
                connectionString.Value(),
                provider.HasValue()
                    ? provider.Value()
                    : "System.Data.SqlClient");
        }
    }
}