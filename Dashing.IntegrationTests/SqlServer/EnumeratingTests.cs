namespace Dashing.IntegrationTests.SqlServer {
    using System;

    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class EnumeratingTests : IClassFixture<SqlServerFixture> {
        private SqlServerFixture fixture;

        public EnumeratingTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public void TestSingleAndFirst() {
            // now fetch them
            var t1 = this.fixture.Session.Query<User>().First();
            Assert.Equal("User_0", t1.Username);

            var t2 = this.fixture.Session.Query<User>().First(u => u.Username == "User_1");
            Assert.Equal("User_1", t2.Username);

            Assert.Throws<InvalidOperationException>(() => this.fixture.Session.Query<User>().Single());

            var t3 = this.fixture.Session.Query<User>().Single(u => u.Username == "User_2");
            Assert.Equal("User_2", t3.Username);

            var t4 = this.fixture.Session.Query<User>().FirstOrDefault();
            Assert.Equal("User_0", t1.Username);

            var t5 = this.fixture.Session.Query<User>().FirstOrDefault(u => u.Username == "Boo");
            Assert.Null(t5);

            var t6 = this.fixture.Session.Query<User>().SingleOrDefault(u => u.Username == "Boo");
            Assert.Null(t6);
        }
    }
}