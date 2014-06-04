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
            var session = config.BeginSession();
            var blogs = session.Query<Blog>().ToList();
            Assert.NotEmpty(blogs);
        }

        [Fact(Skip = "connects to real database")]
        public void ExecuteOneFetchQuery() {
            var config = new CustomConfig();
            var session = config.BeginSession();
            var posts = session.Query<Post>().Fetch(p => p.Blog);
            Assert.NotNull(posts.First().Blog.Title);
        }

        [Fact(Skip = "connects to real database")]
        public void ExecuteNestFetchQuery() {
            var config = new CustomConfig();
            var session = config.BeginSession();
            var comment = session.Query<Comment>().Fetch(c => c.Post.Blog);
            Assert.NotNull(comment.First().Post.Blog.Title);
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new MySqlEngine(), "Server=localhost;Database=tophattest;Uid=root;Pwd=treatme123;") {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}