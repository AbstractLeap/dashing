namespace Dashing.Engine.Dialects {
    using System;
    using System.Configuration;
    using System.Data.SqlClient;

    public class DialectFactory {
        public ISqlDialect Create(ConnectionStringSettings connectionString) {
            switch (connectionString.ProviderName) {
                case "System.Data.SqlClient":
                    var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString.ConnectionString);
                    if (connectionStringBuilder.TypeSystemVersion.StartsWith("SQL Server 200")) {
                        // this is the default for 2000, 2005, 2008
                        return new SqlServerDialect();
                    }

                    // 2012 has nice paging!
                    return new SqlServer2012Dialect();

                case "MySql.Data.MySqlClient":
                    return new MySqlDialect();

                case "System.Data.OleDb":
                    if (connectionString.ConnectionString.Contains("SQLNCLI11")) {
                        return new SqlServer2012Dialect();
                    }
                    else if (connectionString.ConnectionString.Contains("SQLNCLI10")
                        || connectionString.ConnectionString.Contains("SQLNCLI;")) {
                        return new SqlServerDialect();
                    } else if (connectionString.ConnectionString.Contains("MySQLProv")) {
                        return new MySqlDialect();
                    }

                    throw new NotImplementedException(
                        "For OleDb we only recognise Sql Server Native Client and MySQL provider");

                default:
                    throw new NotImplementedException("Sorry, we don't support the \"" + connectionString.ProviderName + "\" provider just yet");
            }
        }
    }
}