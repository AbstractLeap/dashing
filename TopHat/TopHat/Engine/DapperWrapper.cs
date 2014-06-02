namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dapper;

    public class DapperWrapper : IDapperWrapper {
        private readonly IDbConnection connection;

        private readonly IDbTransaction transaction;

        public DapperWrapper(IDbConnection connection, IDbTransaction transaction = null) {
            this.connection = connection;
            this.transaction = transaction;
        }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return this.connection.Execute(sql, param, this.transaction, commandTimeout, commandType);
        }

        public IEnumerable<T> Query<T>(string sql, object param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) {
            return this.connection.Query<T>(sql, param, this.transaction, buffered, commandTimeout, commandType);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            throw new NotImplementedException();
            ////return this.connection.QueryAsync<T>(sql, param, this.transaction, commandTimeout, commandType);
        }
    }
}