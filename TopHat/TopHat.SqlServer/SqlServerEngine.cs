using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TopHat.SqlServer {
	public class SqlServerEngine : EngineBase {
		public override IDbConnection Open(string connectionString) {
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

		public override IEnumerable<T> Query<T>(IDbConnection connection, ISelect<T> query) {
			throw new NotImplementedException();
		}
	}
}