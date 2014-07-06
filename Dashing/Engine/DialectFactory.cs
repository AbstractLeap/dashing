namespace Dashing.Engine {
    using System;
    using System.Configuration;
    using System.Data.SqlClient;

    using Dashing.Engine.Dialects;

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

                default:
                    throw new NotImplementedException();
            }
        }
    }
}