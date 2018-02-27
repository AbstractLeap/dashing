namespace Dashing.Cli {
    using System.Data.Common;
    using System.Linq;

    using Dapper;
    using Dashing.Engine.Dialects;
    using Dashing.Migration;

    public static class DbProviderFactoryExtensions {
        public static bool DatabaseExists(this DbProviderFactory dbProviderFactory, string connectionString, string providerName, ISqlDialect dialect) {
            var connectionStringManipulator = new ConnectionStringManipulator(dbProviderFactory, connectionString);
            using (var connection = dbProviderFactory.CreateConnection())
            {
                connection.ConnectionString = connectionStringManipulator.GetRootConnectionString();
                connection.Open();
                var databaseName = connectionStringManipulator.GetDatabaseName();
                return connection.Query(dialect.CheckDatabaseExists(databaseName)).Any();
            }
        }
    }
}