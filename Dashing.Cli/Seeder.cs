namespace Dashing.Cli {
    using System;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    using Serilog;

    public class Seeder {
        public void Execute(ISeeder seeder, IConfiguration configuration, string connectionString, string providerName) {
            // set up the database connection
            var dialectFactory = new DialectFactory();
            var dialect = dialectFactory.Create(providerName, connectionString);
            var dbProviderFactoryFactory = new DbProviderFactoryFactory();
            var dbProviderFactory = dbProviderFactoryFactory.Create(providerName, connectionString);
            if (!dbProviderFactory.DatabaseExists(connectionString, providerName, dialect)) {
                Log.Logger.Error("Database doesn't exist");
                return;
            }

            // run the script
            var sqlSessionCreator = new SqlDatabase(configuration, dbProviderFactory, connectionString, dialect);
            using (var session = sqlSessionCreator.BeginSession()) {
                seeder.Seed(session);
                session.Complete();
            }
        }
    }
}