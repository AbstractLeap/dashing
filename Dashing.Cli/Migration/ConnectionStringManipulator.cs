namespace Dashing.Migration {
    using System;
    using System.Data.Common;

    public class ConnectionStringManipulator {
        private readonly DbProviderFactory dbProviderFactory;

        private readonly string connectionString;
        
        public ConnectionStringManipulator(DbProviderFactory dbProviderFactory, string connectionString) {
            this.dbProviderFactory = dbProviderFactory;
            this.connectionString = connectionString;
        }

        public string GetRootConnectionString() {
            var builder = this.dbProviderFactory.CreateConnectionStringBuilder();
            if (builder == null) {
                throw new NotSupportedException("Unable to get connection string builder");
            }

            builder.ConnectionString = this.connectionString;
            var databaseProperties = new[] { "Database", "Initial Catalog" };
            foreach (var databaseProperty in databaseProperties) {
                if (builder.ContainsKey(databaseProperty)) {
                    builder.Remove(databaseProperty);
                }
            }

            return builder.ConnectionString;
        }

        public string GetDatabaseName() {
            var builder = this.dbProviderFactory.CreateConnectionStringBuilder();
            if (builder == null) {
                throw new NotSupportedException("Unable to get connection string builder");
            }

            builder.ConnectionString = this.connectionString;
            var databaseProperties = new[] { "Database", "Initial Catalog" };
            foreach (var databaseProperty in databaseProperties) {
                object value;
                if (builder.TryGetValue(databaseProperty, out value)) {
                    if (string.IsNullOrWhiteSpace(value.ToString())) {
                        continue;
                    }

                    return value.ToString();
                }
            }

            throw new NotSupportedException($"Unable to get database name from connection string {this.connectionString}");
        }
    }
}