namespace Dashing.IntegrationTests.Tests {
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.IntegrationTests.Tests.Owned;
    using Dashing.Migration;

    using Xunit;

    public class OwnedTests {
        private readonly SqlDatabase database;

        private const string DatabaseName = "ownedtests";

        public OwnedTests() {
            var dialect = new SqlServer2012Dialect();
            var config = new OwnedConfig();
            this.database = new SqlDatabase(config, SqlClientFactory.Instance, "Server=localhost;Trusted_Connection=True;MultipleActiveResultSets=True", dialect);
            using (var transactionLessSession = this.database.BeginTransactionLessSession())
            {
                
                // create database if exists
                if (transactionLessSession.Dapper.Query(dialect.CheckDatabaseExists(DatabaseName)).Any())
                    {
                        transactionLessSession.Dapper.Execute("drop database " + DatabaseName);
                    }

                transactionLessSession.Dapper.Execute("create database " + DatabaseName);
                transactionLessSession.Dapper.Execute("use " + DatabaseName);
                

                var migrator = new Migrator(
                    new SqlServer2012Dialect(), 
                    new CreateTableWriter(dialect),
                    new AlterTableWriter(dialect),
                    new DropTableWriter(dialect),
                    new StatisticsProvider(null, dialect));
                IEnumerable<string> warnings, errors;
                var createStatement = migrator.GenerateSqlDiff(
                    new List<IMap>(),
                    config.Maps,
                    null,
                    new string[0],
                    new string[0],
                    out warnings,
                    out errors);
                var statements = createStatement.Split(';');
                foreach (var statement in statements.Where(s => !string.IsNullOrWhiteSpace(s.Trim())))
                {
                    transactionLessSession.Dapper.Execute(statement);
                }
            }
        }

        [Fact]
        public async Task RoundTripWorks() {
            using (var session = this.database.BeginTransactionLessSession()) {
                session.Dapper.Execute("use " + DatabaseName);
                var owner = new Owner {
                                          Name = "Foo",
                                          Owned = new Owned.Owned {
                                                                      X = 3,
                                                                      Y = 5
                                                                  }
                                      };
                await session.InsertAsync(owner);
                var ownerSelected = await session.Query<Owner>()
                                                 .SingleAsync(o => o.Id == owner.Id);
                Assert.Equal("Foo", ownerSelected.Name);
                Assert.Equal(3, ownerSelected.Owned.X);
            }
        }
    }
}