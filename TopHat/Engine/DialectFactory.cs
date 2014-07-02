using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Engine
{
    public class DialectFactory
    {
        public ISqlDialect Create(ConnectionStringSettings connectionString)
        {
            switch (connectionString.ProviderName)
            {
                case "System.Data.SqlClient":
                    var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString.ConnectionString);
                    if (connectionStringBuilder.TypeSystemVersion.StartsWith("SQL Server 200")) {
                        // this is the default for 2000, 2005, 2008
                        return new SqlServerDialect();
                    }
                    else {
                        // 2012 has nice paging!
                        return new SqlServer2012Dialect();
                    }

                case "MySql.Data.MySqlClient":
                    return new MySqlDialect();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
