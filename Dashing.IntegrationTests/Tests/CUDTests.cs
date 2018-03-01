namespace Dashing.IntegrationTests.Tests {
    using System;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;
    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class CUDTests {
        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void InsertEnablesTracking(TestSessionWrapper wrapper) {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            wrapper.Session.Insert(user);
            Assert.True(((ITrackedEntity)user).IsTrackingEnabled());
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task InsertAsyncEnablesTracking(TestSessionWrapper wrapper) {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            await wrapper.Session.InsertAsync(user);
            Assert.True(((ITrackedEntity)user).IsTrackingEnabled());
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestInsert(TestSessionWrapper wrapper) {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            wrapper.Session.Insert(user);
            var dbUser = wrapper.Session.Query<User>().First(u => u.EmailAddress == user.EmailAddress);
            Assert.NotNull(dbUser);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestInsertGetsId(TestSessionWrapper wrapper) {
            var user = new User { Username = "Joe", EmailAddress = Guid.NewGuid().ToString(), Password = "blah" };
            wrapper.Session.Insert(user);
            Assert.NotEqual(0, user.UserId);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestMultipleInsertUpdatesIds(TestSessionWrapper wrapper) {
            var user = new User { Username = "Bob", EmailAddress = "asd", Password = "asdf" };
            var user2 = new User { Username = "Bob2", EmailAddress = "asd", Password = "asdf" };
            wrapper.Session.Insert(user, user2);
            Assert.NotEqual(0, user.UserId);
            Assert.NotEqual(0, user2.UserId);
            Assert.NotEqual(user.UserId, user2.UserId);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void UpdateBulk(TestSessionWrapper wrapper) {
            wrapper.Session.Update<User>(u => u.Password = "boo", u => u.Username == "BulkUpdate");
            var user = wrapper.Session.Query<User>().First(u => u.Username == "BulkUpdate");
            Assert.Equal("boo", user.Password);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void DeleteBulk(TestSessionWrapper wrapper) {
            wrapper.Session.Delete<User>(u => u.Username == "BulkDelete");
            var users = wrapper.Session.Query<User>().Where(u => u.Username == "BulkDelete");
            Assert.Empty(users);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestUpdate(TestSessionWrapper wrapper) {
            var user = wrapper.Session.Query<User>().First();
            user.HeightInMeters = 1.7m;
            wrapper.Session.Save(user);
            var dbUser = wrapper.Session.Get<User>(user.UserId);
            Assert.Equal(1.7m, dbUser.HeightInMeters);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestDelete(TestSessionWrapper wrapper) {
            var user = wrapper.Session.Query<User>().First(u => u.Username == "TestDelete");
            wrapper.Session.Delete(user);
            Assert.Empty(wrapper.Session.Query<User>().Where(u => u.Username == "TestDelete"));
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void DateTimeInsertedAndSelectedCorrectly(TestSessionWrapper wrapper) {
            var date = new DateTime(2016, 12, 25, 1, 3, 6, DateTimeKind.Utc);
            var comment = new Comment { Content = "Foo", CommentDate = date };
            wrapper.Session.Insert(comment);
            Assert.Equal(date, comment.CommentDate);
            var fetchedComment = wrapper.Session.Get<Comment>(comment.CommentId);
            Assert.Equal(date, fetchedComment.CommentDate);
        }
    }
}