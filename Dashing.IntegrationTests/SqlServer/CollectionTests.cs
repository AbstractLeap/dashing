namespace Dashing.IntegrationTests.SqlServer {
    using System.Linq;

    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class CollectionTests : IClassFixture<SqlServerFixture> {
        private readonly SqlServerFixture fixture;

        public CollectionTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public void TestCollectionFetch() {
            var posts = this.fixture.Session.Query<Post>().Fetch(p => p.Comments).ToList();
            Assert.True(
                posts.First(p => p.PostId == 1)
                     .Comments.Count(c => c.Content == "Comment_0" || c.Content == "Comment_1" || c.Content == "Nullable User Content") == 3);
        }

        [Fact]
        public void TestMultiCollectionFetch() {
            var posts = this.fixture.Session.Query<Post>().Fetch(p => p.Comments).Fetch(p => p.Tags).Where(p => p.PostId == 1).ToList();
            Assert.True(posts.First().Comments.Count == 3);
            Assert.True(posts.First().Tags.Count == 2);
        }

        [Fact]
        public void TestFetchingEmptyCollection() {
            var emptyBlog = this.fixture.Session.Query<Blog>().First(b => b.Title == "EmptyBlog");
            Assert.Empty(emptyBlog.Posts);
        }

        [Fact]
        public void TestChainedCollectionFetch() {
            var blog = this.fixture.Session.Query<Blog>().FetchMany(p => p.Posts).ThenFetch(p => p.Comments).First();
            Assert.Equal(2, blog.Posts.Count);
            Assert.Equal(3, blog.Posts.First().Comments.Count);
        }

        [Fact]
        public void TestManyFetches() {
            var post =
                this.fixture.Session.Query<Post>()
                    .FetchMany(p => p.Comments)
                    .ThenFetch(c => c.User)
                    .Fetch(p => p.Blog)
                    .Fetch(p => p.Author)
                    .SingleOrDefault(p => p.PostId == 1);
            Assert.True(post != null);
        }

        [Fact]
        public void TestManyToManyThing() {
            var post =
                this.fixture.Session.Query<Post>()
                    .FetchMany(p => p.Tags)
                    .ThenFetch(t => t.Tag)
                    .Fetch(p => p.Blog)
                    .Fetch(p => p.Author)
                    .SingleOrDefault(p => p.PostId == 1);
            Assert.True(post != null);
        }
    }
}