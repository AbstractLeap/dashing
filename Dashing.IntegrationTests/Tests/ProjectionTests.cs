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

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void FetchSinglePropWorks(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>()
                               .Select(p => p.PostId)
                               .AsPaged(10, 10);
            Assert.Equal(10, posts.Taken);
            Assert.True(posts.TotalResults > 10);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void FetchRelationPropWorks(TestSessionWrapper wrapper) {
            var blogs = wrapper.Session.Query<Post>()
                               .Select(p => p.Blog)
                               .AsPaged(10, 10);
            Assert.Equal(10, blogs.Taken);
            Assert.True(blogs.TotalResults > 10);
            Assert.NotNull(
                blogs.Items.First()
                     .Title);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void FetchRelationIdWorks(TestSessionWrapper wrapper) {
            var blogs = wrapper.Session.Query<Post>()
                               .Select(p => p.Blog.BlogId)
                               .AsPaged(10, 10);
            Assert.Equal(10, blogs.Taken);
            Assert.True(blogs.TotalResults > 10);
        }
    }
}