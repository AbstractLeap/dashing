namespace Dashing.IntegrationTests.Tests {
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class AsyncTests {
        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public async Task GetByIdWorks(TestSessionWrapper wrapper) {
            var post = await wrapper.Session.GetAsync<Post, int>(1);
            Assert.Equal(1, post.PostId);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public async Task QueryWorks(TestSessionWrapper wrapper) {
            var posts = await wrapper.Session.Query<Post>().ToListAsync();
            Assert.Equal(20, posts.Count());
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public async Task CollectionWorks(TestSessionWrapper wrapper) {
            var posts = await wrapper.Session.Query<Post>().Fetch(p => p.Comments).ToListAsync();
            Assert.Equal(20, posts.Count());
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public async Task InsertWorks(TestSessionWrapper wrapper) {
            var comment = new Comment { Content = "Foo" };
            await wrapper.Session.InsertAsync(comment);
            Assert.NotEqual(0, comment.CommentId);
        }
    }
}