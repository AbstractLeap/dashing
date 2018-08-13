namespace Dashing.Cli {
    using System;
    using System.Collections.Generic;

    using Dapper;

    using Dashing.CommandLine;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class DatabaseMigrator {
        private ScriptGenerator scriptGenerator;

        public DatabaseMigrator() {
            this.scriptGenerator = new ScriptGenerator();
        }

        public void Execute(IConfiguration configuration, string connectionString, string providerName, IEnumerable<string> tablesToIgnore, IEnumerable<string> indexesToIgnore, IEnumerable<KeyValuePair<string, string>> extraPluralizationWords, bool isVerbose, IAnswerProvider answerProvider) {
            var script = this.scriptGenerator.Generate(configuration, connectionString, providerName, tablesToIgnore, indexesToIgnore, extraPluralizationWords, isVerbose, answerProvider);
            if (string.IsNullOrWhiteSpace(script)) {
                using (new ColorContext(ConsoleColor.Green)) {
                    Console.WriteLine("-- No migration script to run");
                    return;
                }
            }

            // set up the database connection
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(providerName, connectionString);
            var dbProviderFactoryFactory = new DbProviderFactoryFactory();
            var dbProviderFactory = dbProviderFactoryFactory.Create(providerName, connectionString);
            dbProviderFactory.CreateDatabaseIfNotExists(connectionString, providerName, dialect);

            // run the script
            using (var connection = dbProviderFactory.CreateConnection()) {
                connection.ConnectionString = connectionString;
                connection.Open();
                connection.Execute(script);
            }
        }
    }
}