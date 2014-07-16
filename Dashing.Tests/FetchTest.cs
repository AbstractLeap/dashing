namespace Dashing.Tests {
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class FetchTest : BaseQueryWriterTest {
        [Fact]
        public void SingleLevelFetch() {
            var dashing = this.GetDashing();
            var fetch = dashing.Query<Post>().Fetch(p => p.Blog);
            Assert.NotEmpty((fetch as SelectQuery<Post>).Fetches);
        }

        [Fact]
        public void MultipleLevelFetch() {
            var dashing = this.GetDashing();
            var fetch = dashing.Query<Comment>().Fetch(c => c.Post.Blog);
            Assert.NotEmpty((fetch as SelectQuery<Comment>).Fetches);
        }

        [Fact]
        public void CollectionFetch() {
            var dashing = this.GetDashing();
            var collectionFetch = dashing.Query<Post>().Fetch(p => p.Comments);
            Assert.NotEmpty((collectionFetch as SelectQuery<Post>).Fetches);
        }

        [Fact]
        public void CollectionParentFetch() {
            var dashing = this.GetDashing();
            var collectionFetch = dashing.Query<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User);
            Assert.NotNull((collectionFetch as SelectQuery<Post>).CollectionFetches);
        }
    }
}