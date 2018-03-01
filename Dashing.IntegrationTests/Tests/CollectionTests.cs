namespace Dashing.IntegrationTests.Tests {
    using System.Linq;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class CollectionTests {
        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestCollectionFetch(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>().Fetch(p => p.Comments).ToList();
            Assert.True(
                posts.First(p => p.PostId == 1)
                     .Comments.Count(c => c.Content == "Comment_0" || c.Content == "Comment_1" || c.Content == "Nullable User Content") == 3);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestMultiCollectionFetch(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>().Fetch(p => p.Comments).Fetch(p => p.Tags).Where(p => p.PostId == 1).ToList();
            Assert.True(posts.First().Comments.Count == 3);
            Assert.True(posts.First().Tags.Count == 2);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestFetchingEmptyCollection(TestSessionWrapper wrapper) {
            var emptyBlog = wrapper.Session.Query<Blog>().First(b => b.Title == "EmptyBlog");
            Assert.Empty(emptyBlog.Posts);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestChainedCollectionFetch(TestSessionWrapper wrapper) {
            var blog = wrapper.Session.Query<Blog>().FetchMany(p => p.Posts).ThenFetch(p => p.Comments).First();
            Assert.Equal(2, blog.Posts.Count);
            Assert.Equal(3, blog.Posts.First().Comments.Count);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestManyFetches(TestSessionWrapper wrapper) {
            var post =
                wrapper.Session.Query<Post>()
                       .FetchMany(p => p.Comments)
                       .ThenFetch(c => c.User)
                       .Fetch(p => p.Blog)
                       .Fetch(p => p.Author)
                       .SingleOrDefault(p => p.PostId == 1);
            Assert.True(post != null);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void TestManyToManyThing(TestSessionWrapper wrapper) {
            var post =
                wrapper.Session.Query<Post>()
                       .FetchMany(p => p.Tags)
                       .ThenFetch(t => t.Tag)
                       .Fetch(p => p.Blog)
                       .Fetch(p => p.Author)
                       .SingleOrDefault(p => p.PostId == 1);
            Assert.True(post != null);
        }
    }
}