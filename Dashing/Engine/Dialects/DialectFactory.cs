namespace Dashing.Engine.Dialects {
    using System;
    using System.Data.SqlClient;

    public class DialectFactory {
        public ISqlDialect Create(string providerName, string connectionString) {
            if (string.IsNullOrWhiteSpace(providerName)) {
                throw new ArgumentNullException("providerName");
            }

            if (string.IsNullOrWhiteSpace(connectionString)) {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(providerName)) {
                throw new ArgumentException(
                    "Please specify the provider name for that connection string (add providerName=\"System.Data.SqlClient\" to the connection string line in app/web.config)");
            }

            switch (providerName) {
                case "System.Data.SqlClient":
                    var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                    if (connectionStringBuilder.TypeSystemVersion.StartsWith("SQL Server 200")) {
                        // this is the default for 2000, 2005, 2008
                        return new SqlServerDialect();
                    }

                    // 2012 has nice paging!
                    return new SqlServer2012Dialect();

                case "MySql.Data.MySqlClient":
                    return new MySqlDialect();

                case "System.Data.OleDb":
                    if (connectionString.Contains("SQLNCLI11")) {
                        return new SqlServer2012Dialect();
                    }

                    if (connectionString.Contains("SQLNCLI10") || connectionString.Contains("SQLNCLI;")) {
                        return new SqlServerDialect();
                    }

                    if (connectionString.Contains("MySQLProv")) {
                        return new MySqlDialect();
                    }

                    throw new NotImplementedException("For OleDb we only recognise Sql Server Native Client and MySQL provider");

                case "System.Data.SQLite":
                    return new SqliteDialect();

                default:
                    throw new NotImplementedException("Sorry, we don't support the \"" + providerName + "\" provider just yet");
            }
        }
    }
}