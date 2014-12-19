namespace Dashing.IntegrationTests.SqlServer {
    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class TransactionTests : IClassFixture<SqlServerFixture> {
        private readonly SqlServerFixture fixture;

        public TransactionTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public void TestTransactioning() {
            // scrap the current transaction
            this.fixture.Session.Complete();
            this.fixture.Session.Dispose();

            using (var session = this.fixture.Session.Configuration.BeginSession()) {
                session.Dapper.Execute("use " + this.fixture.DatabaseName);
                session.Insert(new User { Username = "james", EmailAddress = "james@polylytics.com" });
                session.Complete();
            }

            using (var session = this.fixture.Session.Configuration.BeginSession()) {
                session.Dapper.Execute("use " + this.fixture.DatabaseName);
                Assert.NotNull(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
                session.Delete<User>(u => u.Username == "james");
                Assert.Null(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
            }

            using (var session = this.fixture.Session.Configuration.BeginSession()) {
                session.Dapper.Execute("use " + this.fixture.DatabaseName);
                Assert.NotNull(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
            }

            this.fixture.Session = this.fixture.Session.Configuration.BeginSession();
        }
    }
}