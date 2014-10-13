namespace Dashing.IntegrationTests.SqlServer {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class ForUpdateTests : IUseFixture<SqlServerFixture> {
        private SqlServerFixture fixture;

        [Fact(Skip = "Has Explicit Delay")]
        public void LockPreventsRead() {
            this.fixture.Session.Complete();
            this.fixture.Session.Dispose();
            var session1 = this.fixture.Session.Configuration.BeginSession();
            session1.Dapper.Execute("use " + this.fixture.DatabaseName);
            var session2 = this.fixture.Session.Configuration.BeginSession();
            session2.Dapper.Execute("use " + this.fixture.DatabaseName);

            var start = DateTime.UtcNow;
            var task1 = Task.Run(
                () => {
                    var post = session1.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
                    session1.Dapper.Execute("waitfor delay '00:00:05';");
                    session1.Dispose();
                });
            Thread.Sleep(1000);
            var task2 = Task.Run(
                () => {
                    var thisPost = session2.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
                    session2.Dispose();
                    return DateTime.UtcNow;
                });
            Task.WhenAll(task1, task2).Wait();

            Assert.True((task2.Result - start).TotalMilliseconds > 4900);

            // do some clear up
            this.fixture.Session = this.fixture.Session.Configuration.BeginSession();
            this.fixture.Session.Dapper.Execute("use " + this.fixture.DatabaseName);
        }

        [Fact(Skip = "Has Explicit Delay")]
        public void LockEnsuresDataReturnedIsUpToDate() {
            this.fixture.Session.Complete();
            this.fixture.Session.Dispose();
            var session1 = this.fixture.Session.Configuration.BeginSession();
            session1.Dapper.Execute("use " + this.fixture.DatabaseName);
            var session2 = this.fixture.Session.Configuration.BeginSession();
            session2.Dapper.Execute("use " + this.fixture.DatabaseName);

            var task1 = Task.Run(
                () => {
                    var post = session1.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
                    post.Title = "I was Locked!";
                    Thread.Sleep(2000);
                    session1.Save(post);
                    session1.Dapper.Execute("waitfor delay '00:00:05';");
                    session1.Complete();
                    session1.Dispose();
                });
            Thread.Sleep(1000);
            var task2 = Task.Run(
                () => {
                    var thisPost = session2.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
                    session2.Dispose();
                    return thisPost.Title;
                });
            Task.WhenAll(task1, task2).Wait();

            Assert.Equal("I was Locked!", task2.Result);

            // do some clear up
            this.fixture.Session = this.fixture.Session.Configuration.BeginSession();
            this.fixture.Session.Dapper.Execute("use " + this.fixture.DatabaseName);
        }

        [Fact(Skip = "Has Explicit Delay")]
        public void LockNotForUpdateReturnsDirty() {
            this.fixture.Session.Complete();
            this.fixture.Session.Dispose();
            var session1 = this.fixture.Session.Configuration.BeginSession();
            session1.Dapper.Execute("use " + this.fixture.DatabaseName);
            var session2 = this.fixture.Session.Configuration.BeginSession();
            session2.Dapper.Execute("use " + this.fixture.DatabaseName);

            var task1 = Task.Run(
                () => {
                    var post = session1.Query<Post>().Where(p => p.PostId == 1).ForUpdate().SingleOrDefault();
                    Thread.Sleep(2000);
                    post.Title = "I was Locked!";
                    session1.Save(post);
                    session1.Dapper.Execute("waitfor delay '00:00:05';");
                    session1.Complete();
                    session1.Dispose();
                });
            Thread.Sleep(1000);
            var task2 = Task.Run(
                () => {
                    var thisPost = session2.Query<Post>().Where(p => p.PostId == 1).SingleOrDefault();
                    session2.Dispose();
                    return thisPost.Title;
                });
            Task.WhenAll(task1, task2).Wait();

            Assert.NotEqual("I was Locked!", task2.Result);

            // do some clear up
            this.fixture.Session = this.fixture.Session.Configuration.BeginSession();
            this.fixture.Session.Dapper.Execute("use " + this.fixture.DatabaseName);
        }

        public void SetFixture(SqlServerFixture data) {
            this.fixture = data;
        }
    }
}