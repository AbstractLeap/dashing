namespace TopHat.Engine {
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public class SqlServerEngine : EngineBase {
        public SqlServerEngine()
            : base(new SqlServerDialect()) { }

        public SqlServerEngine(ISqlDialect dialect)
            : base(dialect) { }

        protected override IDbConnection NewConnection(string connectionString) {
            return new SqlConnection(connectionString);
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