namespace Dashing.Tests {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class FetchTest : BaseQueryWriterTest {
        [Fact]
        public void SingleLevelFetch() {
            var Dashing = this.GetDashing();
            var fetch = Dashing.Query<Post>().Fetch(p => p.Blog);
            Assert.NotEmpty((fetch as SelectQuery<Post>).Fetches);
        }

        [Fact]
        public void MultipleLevelFetch() {
            var Dashing = this.GetDashing();
            var fetch = Dashing.Query<Comment>().Fetch(c => c.Post.Blog);
            Assert.NotEmpty((fetch as SelectQuery<Comment>).Fetches);
        }

        [Fact]
        public void CollectionFetch() {
            var Dashing = this.GetDashing();
            var collectionFetch = Dashing.Query<Post>().Fetch(p => p.Comments);
            Assert.NotEmpty((collectionFetch as SelectQuery<Post>).Fetches);
        }

        [Fact]
        public void CollectionParentFetch() {
            var Dashing = this.GetDashing();
            var collectionFetch = Dashing.Query<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User);
            Assert.NotNull((collectionFetch as SelectQuery<Post>).CollectionFetches);
        }
    }
}