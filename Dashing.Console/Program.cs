namespace Dashing.Console {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using CommandLine;
    using CommandLine.Text;

    using Dashing.Configuration;
    using Dashing.Console.Settings;
    using Dashing.Engine;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tools;
    using Dashing.Tools.Migration;
    using Dashing.Tools.ModelGeneration;
    using Dashing.Tools.ReverseEngineering;

    using DatabaseSchemaReader;
    using DatabaseSchemaReader.DataSchema;

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using ConnectionStringSettings = Dashing.Console.Settings.ConnectionStringSettings;

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
            catch (Exception e) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine("There was a fatal error:");
                    Console.WriteLine(e.Message);
                    Console.Write(e.StackTrace);
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
                DoScript(options.Location, options.Naive, connectionStringSettings, dashingSettings, reverseEngineerSettings);
            }
            else if (options.Migration) {
                DoMigrate(options.Naive, connectionStringSettings, dashingSettings, reverseEngineerSettings);
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
            DashingSettings dashingSettings,
            ReverseEngineerSettings reverseEngineerSettings) {
            if (!naive) {
                using (Color(ConsoleColor.Yellow)) {
                    Console.WriteLine("Non naive migration is experimental. Please check output");
                }
            }

            Console.WriteLine();
            using (Color(ConsoleColor.Yellow)) {
                Console.WriteLine("-- Dashing: Migration Script");
            }
            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", dashingSettings.PathToDll);
            Console.WriteLine("-- Class:    {0}", dashingSettings.ConfigurationName);
            Console.WriteLine();

            // fetch the to state
            IConfiguration config;
            using (new TimedOperation("-- Fetching configuration contents...")) {
                config = LoadConfiguration(dashingSettings, connectionStringSettings);
            }

            IEnumerable<string> warnings, errors;
            var migrationScript = GenerateMigrationScript(connectionStringSettings, dashingSettings, reverseEngineerSettings, config, naive, out warnings, out errors);

            // report errors
            DisplayMigrationWarningsAndErrors(errors, warnings);

            // write it
            using (var writer = string.IsNullOrEmpty(pathOrNull) ? Console.Out : new StreamWriter(File.OpenWrite(pathOrNull))) {
                writer.WriteLine(migrationScript);
            }
        }

        private static void DisplayMigrationWarningsAndErrors(IEnumerable<string> errors, IEnumerable<string> warnings) {
            using (Color(ConsoleColor.Red)) {
                foreach (var error in errors) {
                    Console.Write("-- ");
                    Console.WriteLine(error);
                }
            }

            using (Color(ConsoleColor.Yellow)) {
                foreach (var warning in warnings) {
                    Console.Write("-- ");
                    Console.WriteLine(warning);
                }
            }
        }

        private static void DoMigrate(bool naive, ConnectionStringSettings connectionStringSettings, DashingSettings dashingSettings, ReverseEngineerSettings reverseEngineerSettings) {
            if (!naive) {
                using (Color(ConsoleColor.Yellow)) {
                    Console.WriteLine("Non naive migration is experimental. Please check output");
                }
            }

            // fetch the to state
            IConfiguration config;
            using (new TimedOperation("-- Fetching configuration contents...")) {
                config = LoadConfiguration(dashingSettings, connectionStringSettings);
            }


            IEnumerable<string> warnings, errors;
            var script = GenerateMigrationScript(connectionStringSettings, dashingSettings, reverseEngineerSettings, config, naive, out warnings, out errors);

            // report errors
            DisplayMigrationWarningsAndErrors(errors, warnings);

            if (errors.Any()) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine(
                        "-- Fatal errors encountered: aborting migration. Please review the output.");
                }
            }
            else {
                // migrate it
                var factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
                using (var connection = factory.CreateConnection()) {
                    if (connection == null) {
                        throw new Exception("Could not connect to database");
                    }

                    connection.ConnectionString = connectionStringSettings.ConnectionString;
                    connection.Open();

                    if (string.IsNullOrWhiteSpace(script)) {
                        using (Color(ConsoleColor.Green)) {
                            Console.WriteLine("-- No migration script to run");
                        }
                    }
                    else {
                        using (
                            new TimedOperation(
                                "-- Executing migration script on {0}",
                                connection.ConnectionString))
                        using (var command = connection.CreateCommand()) {
                            using (Color(ConsoleColor.DarkGray)) {
                                Console.WriteLine(script);
                            }

                            command.CommandText = script;
                            command.ExecuteNonQuery();
                        }
                    }

                    // magical crazy time! see http://stackoverflow.com/a/2658326/1255065
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;

                    // now let's call Seed
                    var seederConfig = config as ISeeder;
                    if (seederConfig != null) {
                        using (new TimedOperation("-- Executing seeds"))
                        using (var session = config.BeginSession(connection)) {
                            seederConfig.Seed(session);
                            session.Complete();
                        }
                    }
                }
            }
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args) {
            return ((AppDomain)sender).GetAssemblies().FirstOrDefault(assembly => assembly.FullName == args.Name);
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
            var engineer = new Engineer(reverseEngineerSettings.ExtraPluralizationWords);

            var databaseReader = new DatabaseReader(
                connectionString.ConnectionString,
                connectionString.ProviderName);
            schema = databaseReader.ReadAll();
            var maps = engineer.ReverseEngineer(schema, new DialectFactory().Create(connectionString.ToSystem()), reverseEngineerSettings.GetTablesToIgnore());
            var reverseEngineer = new ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, reverseEngineerSettings.GeneratedNamespace);

            foreach (var source in sources) {
                File.WriteAllText(options.Location + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static void ShowHelpText(CommandLineOptions options) {
            Console.Write(HelpText.AutoBuild(options));
        }

        private static string GenerateMigrationScript(ConnectionStringSettings connectionStringSettings, DashingSettings dashingSettings, ReverseEngineerSettings reverseEngineerSettings, IConfiguration configuration, bool naive, out IEnumerable<string> warnings, out IEnumerable<string> errors) {
            // fetch the from state
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(connectionStringSettings.ToSystem());
            IEnumerable<IMap> fromMaps;
            using (new TimedOperation("-- Reading database contents...")) {
                DatabaseSchema schema;
                var engineer = new Engineer(reverseEngineerSettings.ExtraPluralizationWords);
                var databaseReader = new DatabaseReader(
                    connectionStringSettings.ConnectionString,
                    connectionStringSettings.ProviderName);
                schema = databaseReader.ReadAll();
                fromMaps = engineer.ReverseEngineer(schema, dialect, reverseEngineerSettings.GetTablesToIgnore());
            }

            // set up migrator
            IMigrator migrator;
            if (naive) {
                migrator = new NaiveMigrator(
                    new CreateTableWriter(dialect),
                    new DropTableWriter(dialect),
                    null);
            }
            else {
                migrator = new Migrator(
                    new CreateTableWriter(dialect),
                    new DropTableWriter(dialect),
                    new AlterTableWriter(dialect));
            }

            // run the migrator
            var script = migrator.GenerateSqlDiff(fromMaps, configuration.Maps, out warnings, out errors);

            // TODO: do things with warnings and errors
            return script;
        }

        private static IConfiguration LoadConfiguration(DashingSettings dashingSettings, ConnectionStringSettings connectionStringSettings) {
            // fetch the to state
            var configAssembly = Assembly.LoadFrom(dashingSettings.PathToDll);
            GC.KeepAlive(configAssembly);
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

            // attempt to find the call to ConfigurationManager and overwrite the connection string
            InjectConnectionString(dashingSettings, connectionStringSettings);

            // TODO add in a factory way of generating the config for cases where constructor not empty
            var config = (IConfiguration)Activator.CreateInstance(configType);
            return config;
        }

        private static void InjectConnectionString(DashingSettings dashingSettings, ConnectionStringSettings connectionStringSettings) {
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(dashingSettings.PathToDll);
            var cecilConfigType = assemblyDefinition.MainModule.Types.Single(t => t.FullName == dashingSettings.ConfigurationName);
            var constructor = cecilConfigType.Methods.FirstOrDefault(m => m.IsConstructor && !m.HasParameters); // default constructor
            if (constructor == null) {
                using (Color(ConsoleColor.Red)) {
                    throw new CatchyException("Unable to find a Default Constructor on the Configuration");
                }
            }

            var getConnectionStringCall =
                constructor.Body.Instructions.FirstOrDefault(
                    i =>
                    i.OpCode.Code == Code.Call
                    && i.Operand.ToString()
                    == "System.Configuration.ConnectionStringSettingsCollection System.Configuration.ConfigurationManager::get_ConnectionStrings()");
            if (getConnectionStringCall == null) {
                using (Color(ConsoleColor.Red)) {
                    throw new CatchyException("Unable to find the ConnectionStrings call in the constructor");
                }
            }

            var connectionStringKey = getConnectionStringCall.Next.Operand.ToString();

            // override readonly property of connectionstrings
            typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)
                                                  .SetValue(ConfigurationManager.ConnectionStrings, false);
            ConfigurationManager.ConnectionStrings.Add(
                new System.Configuration.ConnectionStringSettings(
                    connectionStringKey,
                    connectionStringSettings.ConnectionString,
                    connectionStringSettings.ProviderName));
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