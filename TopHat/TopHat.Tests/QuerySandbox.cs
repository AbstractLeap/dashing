namespace TopHat.Tests {
    using System.Linq;

    using TopHat.Configuration;
    using TopHat.MySql;
    using TopHat.Tests.TestDomain;

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

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new MySqlEngine(), "Server=localhost;Database=tophattest;Uid=root;Pwd=treatme123;") {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}