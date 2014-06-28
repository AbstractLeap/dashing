using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Engine
{
    public class DialectFactory
    {
        public ISqlDialect Create(string providerName)
        {
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    return new SqlServerDialect();

                case "MySql.Data.MySqlClient":
                    return new MySqlDialect();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
