namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public class SqlServerEngine : EngineBase {
        public SqlServerEngine()
            : base(new SqlServerDialect()) {
        }

        public SqlServerEngine(ISqlDialect dialect)
            : base(dialect) {
        }

        protected override IDbConnection NewConnection(string connectionString) {
            return new SqlConnection(connectionString);
        }
    }
}