namespace TopHat.Tests {
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using TopHat.Engine;
    using TopHat.Engine.DDL;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class SchemaGenerationSandbox {
        private const string PolyTestConnectionString =
            "Server=tcp:dzarexnyar.database.windows.net;Database=poly-test;User ID=polyadmin@dzarexnyar;Password=Fgg7aEy1bzX8qvs2;Trusted_Connection=False;Encrypt=True;";

        [Fact]
        public void MakeDatSchema() {
            var dialect = new SqlServerDialect();
            var ctw = new CreateTableWriter(dialect);
            var dtw = new DropTableWriter(dialect);
            var config = TH.Configure(new SqlServerEngine(), PolyTestConnectionString).AddNamespaceOf<Post>();

            var dropTables = config.Maps.Select(dtw.DropTableIfExists);
            var createTables = config.Maps.Select(ctw.CreateTable);
            var sqls = dropTables.Concat(createTables).ToArray();

            foreach (var sql in sqls) {
                Debug.WriteLine(sql);
            }

            //// return;

            using (var session = config.BeginSession()) {
                foreach (var sql in sqls) {
                    session.Connection.Execute(sql);
                }
            }
        }
    }
}