using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.SqlServer {
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class CollectionTests : IUseFixture<SqlServerFixture> {
        private SqlServerFixture fixture;

        [Fact]
        public void TestCollectionFetch() {
            var posts = this.fixture.Session.Query<Post>().Fetch(p => p.Comments).ToList();
            Assert.True(
                posts.First(p => p.PostId == 1)
                     .Comments.Count(c => c.Content == "Comment_0" || c.Content == "Comment_1" || c.Content == "Nullable User Content") == 3);
        }

        [Fact]
        public void TestMultiCollectionFetch() {
            var posts = this.fixture.Session.Query<Post>().Fetch(p => p.Comments).Fetch(p => p.Tags).Where(p => p.PostId == 1).ToList();
            Assert.True(posts.First().Comments.Count == 3);
            Assert.True(posts.First().Tags.Count == 2);
        }

        [Fact]
        public void TestFetchingEmptyCollection() {
            var emptyBlog = this.fixture.Session.Query<Blog>().First(b => b.Title == "EmptyBlog");
            Assert.Empty(emptyBlog.Posts);
        }

        [Fact]
        public void TestChainedCollectionFetch() {
            var blog = this.fixture.Session.Query<Blog>().FetchMany(p => p.Posts).ThenFetch(p => p.Comments).First();
            Assert.True(blog.Posts.Count == 2);
            Assert.True(blog.Posts.First().Comments.Count == 3);
        }

        public void SetFixture(SqlServerFixture data) {
            this.fixture = data;
        }
    }
}
