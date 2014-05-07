using System.Data;
using System.Data.SqlClient;
using TopHat.Configuration;

namespace TopHat.SqlServer {
    public class SqlServerConnectionFactory : IConnectionFactory {
        public IDbConnection Open(string connectionString) {
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}