namespace Dashing.IntegrationTests.SqlServer {
    using System;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;
    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class CUDTests : IClassFixture<SqlServerFixture> {
        private readonly SqlServerFixture fixture;

        public CUDTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public void InsertEnablesTracking() {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            this.fixture.Session.Insert(user);
            Assert.True(((ITrackedEntity)user).IsTrackingEnabled());
        }

        [Fact]
        public async Task InsertAsyncEnablesTracking() {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            await this.fixture.Session.InsertAsync(user);
            Assert.True(((ITrackedEntity)user).IsTrackingEnabled());
        }

        [Fact]
        public void TestInsert() {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            this.fixture.Session.Insert(user);
            var dbUser = this.fixture.Session.Query<User>().First(u => u.EmailAddress == user.EmailAddress);
            Assert.NotNull(dbUser);
        }

        [Fact]
        public void TestInsertGetsId() {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            this.fixture.Session.Insert(user);
            Assert.NotEqual(0, user.UserId);
        }

        [Fact]
        public void TestMultipleInsertUpdatesIds() {
            var user = new User { Username = "Bob", EmailAddress = "asd", Password = "asdf" };
            var user2 = new User { Username = "Bob2", EmailAddress = "asd", Password = "asdf" };
            this.fixture.Session.Insert(user, user2);
            Assert.NotEqual(0, user.UserId);
            Assert.NotEqual(0, user2.UserId);
            Assert.NotEqual(user.UserId, user2.UserId);
        }

        [Fact]
        public void UpdateBulk() {
            this.fixture.Session.Update<User>(u => u.Password = "boo", u => u.Username == "BulkUpdate");
            var user = this.fixture.Session.Query<User>().First(u => u.Username == "BulkUpdate");
            Assert.Equal("boo", user.Password);
        }

        [Fact]
        public void DeleteBulk() {
            this.fixture.Session.Delete<User>(u => u.Username == "BulkDelete");
            var users = this.fixture.Session.Query<User>().Where(u => u.Username == "BulkDelete");
            Assert.Empty(users);
        }

        [Fact]
        public void TestUpdate() {
            var user = this.fixture.Session.Query<User>().First();
            user.HeightInMeters = 1.7m;
            this.fixture.Session.Save(user);
            var dbUser = this.fixture.Session.Get<User>(user.UserId);
            Assert.Equal(1.7m, dbUser.HeightInMeters);
        }

        [Fact]
        public void TestDelete() {
            var user = this.fixture.Session.Query<User>().First(u => u.Username == "TestDelete");
            this.fixture.Session.Delete(user);
            Assert.Empty(this.fixture.Session.Query<User>().Where(u => u.Username == "TestDelete"));
        }
    }
}