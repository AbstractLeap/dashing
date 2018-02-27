namespace Dashing.Cli {
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Migration;
    using Dashing.ReverseEngineering;
    using Dashing.SchemaReading;

    public class ScriptGenerator {
        public string Generate(IConfiguration configuration, string connectionString, string providerName, IEnumerable<string> tablesToIgnore, IEnumerable<string> indexesToIgnore, IEnumerable<KeyValuePair<string, string>> extraPluralizationWords, bool isVerbose, IAnswerProvider answerProvider) {

            // set up the database connection
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(providerName, connectionString);
            var dbProviderFactoryFactory = new DbProviderFactoryFactory();
            var dbProviderFactory = dbProviderFactoryFactory.Create(providerName, connectionString);

            IEnumerable<IMap> fromMaps;
            if (!dbProviderFactory.DatabaseExists(connectionString, providerName, dialect)) {
                fromMaps = Enumerable.Empty<IMap>();
                return this.GenerateScript(fromMaps, configuration.Maps, dialect, new NullStatisticsProvider(), answerProvider, tablesToIgnore, indexesToIgnore, isVerbose);
            } else {
                // get the schema from the existing database
                var schemaReaderFactory = new SchemaReaderFactory();
                var schemaReader = schemaReaderFactory.GetSchemaReader(providerName);
                var connectionStringManipulator = new ConnectionStringManipulator(dbProviderFactory, connectionString);
                using (var connection = dbProviderFactory.CreateConnection()) {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    var schema = schemaReader.Read(connection, connectionStringManipulator.GetDatabaseName());

                    // reverse engineer the maps
                    var engineer = new Engineer(extraPluralizationWords.Union(configuration.Maps.Select(m => new KeyValuePair<string, string>(m.Type.Name, m.Table)))); // we use our configuration to inform us as to the correct naming of tables
                    fromMaps = engineer.ReverseEngineer(schema, dialect, tablesToIgnore, answerProvider, false);
                    return this.GenerateScript(fromMaps, configuration.Maps, dialect, new StatisticsProvider(connection, dialect), answerProvider, tablesToIgnore, indexesToIgnore, isVerbose);
                }
            }
        }

        private string GenerateScript(IEnumerable<IMap> fromMaps, IEnumerable<IMap> configurationMaps, ISqlDialect dialect, IStatisticsProvider statisticsProvider, IAnswerProvider answerProvider, IEnumerable<string> tablesToIgnore, IEnumerable<string> indexesToIgnore, bool isVerbose) {
            var migrator = new Migrator(
                dialect,
                new CreateTableWriter(dialect),
                new AlterTableWriter(dialect),
                new DropTableWriter(dialect),
                statisticsProvider);
            IEnumerable<string> warnings;
            IEnumerable<string> errors;
            return migrator.GenerateSqlDiff(
                fromMaps,
                configurationMaps,
                answerProvider,
                new ConsoleLogger(isVerbose),
                indexesToIgnore,
                tablesToIgnore,
                out warnings,
                out errors);
        }
    }
}