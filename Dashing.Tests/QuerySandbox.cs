namespace Dashing.Tests {
    using System;
    using System.Configuration;
    using System.Linq;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class QuerySandbox {
        [Fact(Skip = "connects to real database")]
        public void ExecuteSimpleQuery() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var blogs = session.Query<Blog>().ToList();
                Assert.NotEmpty(blogs);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void ExecuteOneFetchQuery() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var posts = session.Query<Post>().Fetch(p => p.Blog);
                Assert.NotNull(posts.First().Blog.Title);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void ExecuteNestFetchQuery() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var comment = session.Query<Comment>().Fetch(c => c.Post.Blog);
                Assert.NotNull(comment.First().Post.Blog.Title);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestInsert() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var post = new User { Username = "Joe", EmailAddress = "m@j.com", Password = "blah" };
                session.Insert(post);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestInsertUpdatesId() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var user = new User { Username = "Bob", EmailAddress = "asd", Password = "asdf" };
                session.Insert(user);
                Assert.NotEqual(0, user.UserId);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestMultipleInsertUpdatesIds() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var user = new User { Username = "Bob", EmailAddress = "asd", Password = "asdf" };
                var user2 = new User { Username = "Bob2", EmailAddress = "asd", Password = "asdf" };
                session.Insert(user, user2);
                Assert.NotEqual(0, user.UserId);
                Assert.NotEqual(0, user2.UserId);
                Assert.NotEqual(user.UserId, user2.UserId);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void UpdateBulk() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                session.Update<User>(u => u.Password = "boo", u => u.Username == "Bob2");
            }
        }

        [Fact(Skip = "connects to real database")]
        public void DeleteBulk() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                session.Delete<User>(u => u.Username == "Bob2");
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestSingleAndFirst() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var user = new User { Username = "Bob", EmailAddress = "asd", Password = "asdf" };
                var user2 = new User { Username = "Bob2", EmailAddress = "asd", Password = "asdf" };
                session.Insert(user, user2);

                // now fetch them
                var t1 = session.Query<User>().First();
                Assert.Equal("Bob", t1.Username);

                var t2 = session.Query<User>().First(u => u.Username == "Bob2");
                Assert.Equal("Bob2", t2.Username);

                Assert.Throws<InvalidOperationException>(() => session.Query<User>().Single());

                var t3 = session.Query<User>().Single(u => u.Username == "Bob2");
                Assert.Equal("Bob2", t3.Username);

                var t4 = session.Query<User>().FirstOrDefault();
                Assert.Equal("Bob", t1.Username);

                var t5 = session.Query<User>().FirstOrDefault(u => u.Username == "james");
                Assert.Null(t5);

                var t6 = session.Query<User>().SingleOrDefault(u => u.Username == "james");
                Assert.Null(t6);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestUpdate() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var user = session.Query<User>().AsTracked().First();
                user.HeightInMeters = 1.7m;
                session.Update(user);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestDelete() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var user = session.Query<User>().AsTracked().First();
                session.Delete(user);
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestCollectionFetch() {
            var config = new CustomConfig();
            using (var session = config.BeginSession()) {
                var posts = session.Query<Post>().Fetch(p => p.Comments).ToList();
            }
        }

        [Fact(Skip = "connects to real database")]
        public void TestTransactioning() {
            var dialect = new SqlServerDialect();
            var dropTableWriter = new DropTableWriter(dialect);
            var createTableWriter = new CreateTableWriter(dialect);
            var config = NeedToDash.Configure(SchemaGenerationSandbox.PolyTestConnectionString).AddNamespaceOf<Post>();

            using (var session = config.BeginSession()) {
                foreach (var map in config.Maps) {
                    session.Connection.Execute(dropTableWriter.DropTableIfExists(map));
                    session.Connection.Execute(createTableWriter.CreateTable(map));
                }

                session.Insert(new User { Username = "james", EmailAddress = "james@polylytics.com" });
                session.Complete();
            }

            using (var session = config.BeginSession()) {
                Assert.NotNull(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
                session.Delete<User>(u => u.Username == "james");
                Assert.Null(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
            }

            using (var session = config.BeginSession()) {
                Assert.NotNull(session.Query<User>().SingleOrDefault(u => u.Username == "james"));
            }
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new ConnectionStringSettings("Default", "Server=localhost;Database=Dashingtest;Uid=root;Pwd=treatme123;", "MySql.Data.MySqlClient")) {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}