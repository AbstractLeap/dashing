namespace Dashing.IntegrationTests.SqlServer {
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class AsyncTests : IClassFixture<SqlServerFixture> {
        private readonly SqlServerFixture fixture;

        public AsyncTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public async Task GetByIdWorks() {
            var post = await this.fixture.Session.GetAsync<Post, int>(1);
            Assert.Equal(1, post.PostId);
        }

        [Fact]
        public async Task QueryWorks() {
            var posts = await this.fixture.Session.Query<Post>().ToListAsync();
            Assert.Equal(20, posts.Count());
        }

        [Fact]
        public async Task CollectionWorks() {
            var posts = await this.fixture.Session.Query<Post>().Fetch(p => p.Comments).ToListAsync();
            Assert.Equal(20, posts.Count());
        }

        [Fact]
        public async Task InsertWorks() {
            var comment = new Comment { Content = "Foo" };
            await this.fixture.Session.InsertAsync(comment);
            Assert.NotEqual(0, comment.CommentId);
        }
    }
}