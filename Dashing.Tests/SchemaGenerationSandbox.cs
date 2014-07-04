namespace Dashing.Tests {
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using Dashing.Engine;
    using Dashing.Engine.DDL;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class SchemaGenerationSandbox {
        private static ConnectionStringSettings PolyTestConnectionString = new ConnectionStringSettings("Default", "Server=tcp:dzarexnyar.database.windows.net;Database=poly-test;User ID=polyadmin@dzarexnyar;Password=Fgg7aEy1bzX8qvs2;Trusted_Connection=False;Encrypt=True;", "System.Data.SqlClient");

        [Fact]
        public void MakeDatSchema() {
            var dialect = new SqlServerDialect();
            var ctw = new CreateTableWriter(dialect);
            var dtw = new DropTableWriter(dialect);
            var config = NeedToDash.Configure(PolyTestConnectionString).AddNamespaceOf<Post>();

            var dropTables = config.Maps.Select(dtw.DropTableIfExists);
            var createTables = config.Maps.Select(ctw.CreateTable);
            var sqls = dropTables.Concat(createTables).ToArray();

            foreach (var sql in sqls) {
                Debug.WriteLine(sql);
            }

            ////using (var session = config.BeginSession()) {
            ////    foreach (var sql in sqls) {
            ////        session.Connection.Execute(sql);
            ////    }
            ////}
        }
    }
}