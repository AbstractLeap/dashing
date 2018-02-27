namespace Dashing.Cli {
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;

    using MySql.Data.MySqlClient;

    public class DbProviderFactoryFactory {
        public DbProviderFactory Create(string providerName, string connectionString) {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentNullException("providerName");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            switch (providerName)
            {
                case "System.Data.SqlClient":
                    return SqlClientFactory.Instance;

                case "MySql.Data.MySqlClient":
                    return MySqlClientFactory.Instance;

                default:
                    throw new NotImplementedException("Sorry, we don't support the \"" + providerName + "\" provider just yet");
            }
        }
    }
}