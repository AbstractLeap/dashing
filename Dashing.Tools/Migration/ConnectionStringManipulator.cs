namespace Dashing.Tools.Migration {
    using System;
    using System.Configuration;
    using System.Data.Common;

    public class ConnectionStringManipulator {
        private readonly ConnectionStringSettings connectionStringSettings;

        public ConnectionStringManipulator(ConnectionStringSettings connectionStringSettings) {
            this.connectionStringSettings = connectionStringSettings;
        }

        public ConnectionStringSettings GetRootConnectionString() {
            var builder = DbProviderFactories.GetFactory(this.connectionStringSettings.ProviderName).CreateConnectionStringBuilder();
            if (builder == null) {
                throw new NotSupportedException("Unable to get connection string builder for " + this.connectionStringSettings.ProviderName);
            }

            builder.ConnectionString = this.connectionStringSettings.ConnectionString;
            var databaseProperties = new[] { "Database", "Initial Catalog" };
            foreach (var databaseProperty in databaseProperties) {
                if (builder.ContainsKey(databaseProperty)) {
                    builder.Remove(databaseProperty);
                }
            }

            return new ConnectionStringSettings(this.connectionStringSettings.Name, builder.ConnectionString, this.connectionStringSettings.ProviderName);
        }

        public string GetDatabaseName() {
            var builder = DbProviderFactories.GetFactory(this.connectionStringSettings.ProviderName).CreateConnectionStringBuilder();
            if (builder == null) {
                throw new NotSupportedException("Unable to get connection string builder for " + this.connectionStringSettings.ProviderName);
            }

            builder.ConnectionString = this.connectionStringSettings.ConnectionString;
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

            throw new NotSupportedException(string.Format("Unable to get database name from connection string named {0} ({1})", this.connectionStringSettings.Name, this.connectionStringSettings.ConnectionString));
        }
    }
}