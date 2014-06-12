namespace TopHat.Tests {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class FetchTest : BaseQueryWriterTest {
        [Fact]
        public void SingleLevelFetch() {
            var topHat = this.GetTopHat();
            var fetch = topHat.Query<Post>().Fetch(p => p.Blog);
            Assert.NotEmpty((fetch as SelectQuery<Post>).Fetches);
        }

        [Fact]
        public void MultipleLevelFetch() {
            var topHat = this.GetTopHat();
            var fetch = topHat.Query<Comment>().Fetch(c => c.Post.Blog);
            Assert.NotEmpty((fetch as SelectQuery<Comment>).Fetches);
        }

        [Fact]
        public void CollectionFetch() {
            var topHat = this.GetTopHat();
            var collectionFetch = topHat.Query<Post>().Fetch(p => p.Comments);
            Assert.NotEmpty((collectionFetch as SelectQuery<Post>).Fetches);
        }

        [Fact]
        public void CollectionParentFetch() {
            var topHat = this.GetTopHat();
            var collectionFetch = topHat.Query<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User);
            Assert.NotNull((collectionFetch as SelectQuery<Post>).CollectionFetches);
        }
    }
}