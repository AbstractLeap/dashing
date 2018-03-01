namespace Dashing.IntegrationTests.Tests {
    using System.Linq;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class SelectTests {
        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void WhereEnumerableWorks(TestSessionWrapper wrapper) {
            var ids = new long[] { 1, 2, 3 };
            var posts = wrapper.Session.Query<Post>().Where(p => ids.Contains(p.PostId)).ToList();
            Assert.Equal(3, posts.Count);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void WhereAnyWorks(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>().Where(p => p.Comments.Any(c => c.Content == "Comment_1")).ToList();
            Assert.Single(posts);
        }
    }
}