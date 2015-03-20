namespace Dashing.Console {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
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

    using Mono.Cecil;
    using Mono.Cecil.Cil;

    using ConnectionStringSettings = Dashing.Console.Settings.ConnectionStringSettings;

    internal class Program {
        private static object configObject;

        private static IAnswerProvider consoleAnswerProvider;

        private static bool isVerbose;

        private static void Main(string[] args) {
             ConfigureAssemblyResolution();

            try {
                InnerMain(args);
            }
            catch (CatchyException e) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine(e.Message);
                }
            }
            catch (ReflectionTypeLoadException rtle) {
                foreach (var le in rtle.LoaderExceptions) {
                    Console.WriteLine(le.Message);
                }
            }
            catch (TargetInvocationException e) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine();
                    Console.WriteLine("Encountered a problem instantiating the configuration object");
                }

                var rtle = e.InnerException as ReflectionTypeLoadException;
                if (rtle != null) {
                    foreach (var le in rtle.LoaderExceptions) {
                        Console.WriteLine(le.Message);
                    }
                }
            }
            catch (Exception e) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine("Caught unhandled {0}", e.GetType()
                                                               .Name);
                    Console.WriteLine(e.Message);
                    var ee = e;
                    while ((ee = ee.InnerException) != null) {
                        Console.WriteLine(ee.Message);
                    }
                }

                using (Color(ConsoleColor.Gray)) {
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        private static void ConfigureAssemblyResolution() { // http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
            AppDomain.CurrentDomain.AssemblyResolve += (sender, iargs) => {
                var assemblyName = new AssemblyName(iargs.Name);

                // look in app domain
                var loaded = AppDomain.CurrentDomain.GetAssemblies()
                                      .SingleOrDefault(a => a.FullName == assemblyName.FullName);
                if (loaded != null) {
                    Trace("Loaded assembly {0} from existing AppDomain, {1}", assemblyName, loaded.Location);
                    return loaded;
                }

                // look in embedded resources
                var resourceName = "Dashing.Console.lib." + assemblyName.Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly()
                                            .GetManifestResourceStream(resourceName)) {
                    if (stream != null) {
                        Trace("Loaded assembly {0} from embedded resources", assemblyName);
                        var assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }

                // we couldn't find it, look on disk
                var path = assemblyName.Name + ".dll";
                if (File.Exists(path)) {
                    Trace("Loaded assembly {0} from disk", assemblyName);
                    var assemblyData = File.ReadAllBytes(path);
                    return Assembly.Load(assemblyData);
                }

                return null;
            };
        }

        private static void InnerMain(string[] args) {
            var options = new CommandLineOptions();

            if (!Parser.Default.ParseArguments(args, options)) {
                ShowHelpText(options);
                return;
            }

            isVerbose = options.Verbose;

            // prevalidation
            if (string.IsNullOrWhiteSpace(options.ConfigPath)) {
                throw new CatchyException("You must specify a configuration path or a project name");
            }

            if (!File.Exists(options.ConfigPath)) {
                throw new CatchyException("Could not locate configuration file {0}", options.ConfigPath);
            }

            // dependency init
            consoleAnswerProvider = new ConsoleAnswerProvider("~" + Path.GetFileNameWithoutExtension(options.ConfigPath) + ".answers");

            // parse all of the configuration stuffs
            ConnectionStringSettings connectionStringSettings;
            DashingSettings dashingSettings;
            ReverseEngineerSettings reverseEngineerSettings;
            ParseIni(options, out connectionStringSettings, out dashingSettings, out reverseEngineerSettings);

            // postvalidation
            if (!File.Exists(dashingSettings.PathToDll)) {
                throw new CatchyException("Could not locate {0}", dashingSettings.PathToDll);
            }

            // load the configuration NOW and try to inherit its version of Dashing, Dapper, etc
            var configAssembly = Assembly.LoadFrom(dashingSettings.PathToDll);
            GC.KeepAlive(configAssembly);
            configObject = LoadConfiguration(configAssembly, dashingSettings, connectionStringSettings);

            // now decide what to do
            if (options.Script) {
                DoScript(options.Location, options.Naive, connectionStringSettings, dashingSettings, reverseEngineerSettings);
            }
            else if (options.Seed) {
                DoSeed(connectionStringSettings);
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

        private static void ParseIni(CommandLineOptions options, out ConnectionStringSettings connectionStringSettings, out DashingSettings dashingSettings, out ReverseEngineerSettings reverseEngineerSettings) {
            var config = IniParser.Parse(options.ConfigPath);

            connectionStringSettings = new ConnectionStringSettings();
            connectionStringSettings = IniParser.AssignTo(config["Database"], connectionStringSettings);

            dashingSettings = new DashingSettings();
            dashingSettings = IniParser.AssignTo(config["Dashing"], dashingSettings);

            reverseEngineerSettings = new ReverseEngineerSettings();
            reverseEngineerSettings = IniParser.AssignTo(config["ReverseEngineer"], reverseEngineerSettings);
        }

        private static object LoadConfiguration(Assembly configAssembly, DashingSettings dashingSettings, ConnectionStringSettings connectionStringSettings) {
            // fetch the to state
            var configType = configAssembly.DefinedTypes.SingleOrDefault(t => t.FullName == dashingSettings.ConfigurationName);

            if (configType == null) {
                using (Color(ConsoleColor.Red)) {
                    var candidates = configAssembly.DefinedTypes.Where(t => t.GetInterface(typeof(IConfiguration).FullName) != null)
                                                   .ToArray();
                    if (candidates.Any()) {
                        throw new CatchyException("Could not locate {0}, but found candidates: {1}", dashingSettings.ConfigurationName, string.Join(", ", candidates.Select(c => c.FullName)));
                    }

                    throw new CatchyException("Could not locate {0}, and found no candidate configurations", dashingSettings.ConfigurationName);
                }
            }

            // attempt to find the call to ConfigurationManager and overwrite the connection string
            InjectConnectionString(dashingSettings, connectionStringSettings);

            // TODO add in a factory way of generating the config for cases where constructor not empty
            return Activator.CreateInstance(configType);
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

            var getConnectionStringCall = constructor.Body.Instructions.FirstOrDefault(i => i.OpCode.Code == Code.Call && i.Operand.ToString() == "System.Configuration.ConnectionStringSettingsCollection System.Configuration.ConfigurationManager::get_ConnectionStrings()");
            if (getConnectionStringCall == null) {
                using (Color(ConsoleColor.Red)) {
                    throw new CatchyException("Unable to find the ConnectionStrings call in the constructor");
                }
            }

            var connectionStringKey = getConnectionStringCall.Next.Operand.ToString();

            // override readonly property of connectionstrings
            var readOnlyField = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            if (readOnlyField != null) {
                readOnlyField.SetValue(ConfigurationManager.ConnectionStrings, false);
            }

            // remove any existing
            if (ConfigurationManager.ConnectionStrings[connectionStringKey] != null) {
                ConfigurationManager.ConnectionStrings.Remove(connectionStringKey);
            }

            // and add in the one from our ini
            ConfigurationManager.ConnectionStrings.Add(new System.Configuration.ConnectionStringSettings(connectionStringKey, connectionStringSettings.ConnectionString, connectionStringSettings.ProviderName));
        }

        private static void DoScript(string pathOrNull, bool naive, ConnectionStringSettings connectionStringSettings, DashingSettings dashingSettings, ReverseEngineerSettings reverseEngineerSettings) {
            DisplayMigrationHeader(naive, dashingSettings);

            // fetch the to state
            var config = (IConfiguration)configObject;

            IEnumerable<string> warnings,
                                errors;
            var migrationScript = GenerateMigrationScript(connectionStringSettings, reverseEngineerSettings, config, naive, out warnings, out errors);

            // report errors
            DisplayMigrationWarningsAndErrors(errors, warnings);

            if (string.IsNullOrWhiteSpace(migrationScript)) {
                migrationScript = "-- Nothing to be migrated";
            }

            // write it
            using (var writer = string.IsNullOrEmpty(pathOrNull)
                                    ? Console.Out
                                    : new StreamWriter(File.OpenWrite(pathOrNull))) {
                writer.WriteLine(migrationScript);
            }
        }

        private static void DisplayMigrationHeader(bool naive, DashingSettings dashingSettings) {
            using (Color(ConsoleColor.Yellow)) {
                Console.WriteLine("-- Dashing: Migration Script");
            }

            Console.WriteLine("-- -------------------------------");
            Console.WriteLine("-- Assembly: {0}", dashingSettings.PathToDll);
            Console.WriteLine("-- Class:    {0}", dashingSettings.ConfigurationName);
            Console.WriteLine("-- ");

            if (!naive) {
                using (Color(ConsoleColor.Yellow)) {
                    Console.WriteLine("-- -------------------------------");
                    Console.WriteLine("-- Migration is experimental:");
                    Console.WriteLine("-- Please check the output!");
                    Console.WriteLine("-- -------------------------------");
                }
            }
        }

        private static bool DisplayMigrationWarningsAndErrors(IEnumerable<string> errors, IEnumerable<string> warnings) {
            bool shouldExit = false;

            using (Color(ConsoleColor.Red)) {
                foreach (var error in errors) {
                    shouldExit = true;
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

            return shouldExit;
        }

        private static void DoMigrate(bool naive, ConnectionStringSettings connectionStringSettings, DashingSettings dashingSettings, ReverseEngineerSettings reverseEngineerSettings) {
            DisplayMigrationHeader(naive, dashingSettings);

            // fetch the to state
            var config = (IConfiguration)configObject;

            IEnumerable<string> warnings,
                                errors;
            var script = GenerateMigrationScript(connectionStringSettings, reverseEngineerSettings, config, naive, out warnings, out errors);

            if (DisplayMigrationWarningsAndErrors(errors, warnings)) {
                using (Color(ConsoleColor.Red)) {
                    Console.WriteLine("-- Fatal errors encountered: aborting migration. Please review the output.");
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
                        using (new TimedOperation("-- Executing migration script on {0}", connection.ConnectionString)) {
                            using (var command = connection.CreateCommand()) {
                                using (Color(ConsoleColor.DarkGray)) {
                                    Console.WriteLine();
                                    Console.WriteLine(script);
                                    Console.WriteLine();
                                }

                                command.CommandText = script;
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    // now let's call Seed
                    var seederConfig = config as ISeeder;
                    if (seederConfig != null) {
                        using (new TimedOperation("-- Executing seeds")) {
                            using (var session = config.BeginSession(connection)) {
                                seederConfig.Seed(session);
                                session.Complete();
                            }
                        }
                    }
                }
            }
        }

        private static void DoSeed(ConnectionStringSettings connectionStringSettings) {
            // fetch the to state
            var config = (IConfiguration)configObject;

            var factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
            using (var connection = factory.CreateConnection()) {
                if (connection == null) {
                    throw new Exception("Could not connect to database");
                }

                connection.ConnectionString = connectionStringSettings.ConnectionString;
                connection.Open();

                // now let's call Seed
                var seederConfig = config as ISeeder;
                if (seederConfig != null) {
                    using (new TimedOperation("-- Executing seeds")) {
                        using (var session = config.BeginSession(connection)) {
                            seederConfig.Seed(session);
                            session.Complete();
                        }
                    }
                }
            }
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

            var databaseReader = new DatabaseReader(connectionString.ConnectionString, connectionString.ProviderName);
            schema = databaseReader.ReadAll();
            var maps = engineer.ReverseEngineer(schema, new DialectFactory().Create(connectionString.ToSystem()), reverseEngineerSettings.GetTablesToIgnore(), consoleAnswerProvider);
            var reverseEngineer = new ModelGenerator();
            var sources = reverseEngineer.GenerateFiles(maps, schema, reverseEngineerSettings.GeneratedNamespace, consoleAnswerProvider);

            foreach (var source in sources) {
                File.WriteAllText(options.Location + "\\" + source.Key + ".cs", source.Value);
            }
        }

        private static void ShowHelpText(CommandLineOptions options) {
            Console.Write(HelpText.AutoBuild(options));
        }

        private static string GenerateMigrationScript(ConnectionStringSettings connectionStringSettings, ReverseEngineerSettings reverseEngineerSettings, IConfiguration configuration, bool naive, out IEnumerable<string> warnings, out IEnumerable<string> errors) {
            // fetch the from state
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(connectionStringSettings.ToSystem());

            DatabaseSchema schema;
            using (new TimedOperation("-- Reading database contents...")) {
                var databaseReader = new DatabaseReader(connectionStringSettings.ConnectionString, connectionStringSettings.ProviderName);
                schema = databaseReader.ReadAll();
            }

            IEnumerable<IMap> fromMaps;
            using (new TimedOperation("-- Reverse engineering...")) {
                Console.WriteLine();
                var engineer = new Engineer(reverseEngineerSettings.ExtraPluralizationWords);
                fromMaps = engineer.ReverseEngineer(schema, dialect, reverseEngineerSettings.GetTablesToIgnore(), consoleAnswerProvider);
                Console.Write("-- ");
            }

            // set up migrator
            IMigrator migrator;
            if (naive) {
                migrator = new NaiveMigrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), null);
            }
            else {
                migrator = new Migrator(new CreateTableWriter(dialect), new DropTableWriter(dialect), new AlterTableWriter(dialect));
            }

            // run the migrator
            string script;
            using (new TimedOperation("-- Generating diff...")) {
                script = migrator.GenerateSqlDiff(fromMaps, configuration.Maps, consoleAnswerProvider, Trace, reverseEngineerSettings.GetIndexesToIgnore(), out warnings, out errors);
            }

            // TODO: do things with warnings and errors
            return script;
        }

        private static void Trace(string text) {
            if (!isVerbose) {
                return;
            }

            using (Color(ConsoleColor.Gray)) {
                Console.WriteLine(text);
            }
        }

        private static void Trace(string format, params object[] args) {
            Trace(string.Format(format, args));
        }

        private static ColorContext Color(ConsoleColor color) {
            return new ColorContext(color);
        }
    }
}