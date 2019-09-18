namespace Dashing.IntegrationTests.Tests {
    using System.Linq;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class ProjectionTests {
        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void NoFetchProjectionWorks(TestSessionWrapper wrapper) {
            var ids = new long[] { 1, 2, 3 };
            var posts = wrapper.Session.Query<Post>()
                               .Where(p => ids.Contains(p.PostId))
                               .Select(p => new { p.Title })
                               .ToList();
            Assert.Equal(3, posts.Count);
            Assert.NotNull(posts[0].Title);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void FetchProjectionWorks(TestSessionWrapper wrapper) {
            var ids = new long[] { 1, 2, 3 };
            var posts = wrapper.Session.Query<Post>()
                               .Where(p => ids.Contains(p.PostId))
                               .Select(p => new { p.Title, BlogTitle = p.Blog.Title })
                               .ToList();
            Assert.Equal(3, posts.Count);
            Assert.NotNull(posts[0].Title);
            Assert.NotNull(posts[0].BlogTitle);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void FetchProjectionPagedWorks(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>()
                               .Select(p => new { p.Title, BlogTitle = p.Blog.Title })
                               .AsPaged(10, 10);
            Assert.Equal(10, posts.Taken);
            Assert.True(posts.TotalResults > 10);
            Assert.NotNull(posts.Items.ElementAt(0).Title);
            Assert.NotNull(posts.Items.ElementAt(0).BlogTitle);
        }
    }
}