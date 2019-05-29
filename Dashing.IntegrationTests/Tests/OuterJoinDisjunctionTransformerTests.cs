namespace Dashing.IntegrationTests.Tests {
    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class OuterJoinDisjunctionTransformerTests {
        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public void OuterJoinDisjunctionWorks(TestSessionWrapper wrapper) {
            var posts = wrapper.Session.Query<Post>()
                               .Where(p => p.Blog.Title == "Blog_1" || p.Blog.Title == "Blog_2");
            Assert.NotEmpty(posts);
        }
    }
}