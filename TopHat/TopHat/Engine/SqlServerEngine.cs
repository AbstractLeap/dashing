namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class SqlServerEngine : EngineBase {
        protected override IDbConnection NewConnection(string connectionString) {
            return new SqlConnection(connectionString);
        }

        public override IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query) {
            throw new NotImplementedException();
        }

        public override int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query) {
            throw new NotImplementedException();
        }

        public override int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query) {
            throw new NotImplementedException();
        }

        public override int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query) {
            throw new NotImplementedException();
        }
    }
}