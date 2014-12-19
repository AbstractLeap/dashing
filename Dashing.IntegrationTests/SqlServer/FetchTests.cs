using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.SqlServer {
    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class FetchTests : IClassFixture<SqlServerFixture> {
        private SqlServerFixture fixture;

        public FetchTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public void ExecuteSimpleQuery() {
            var blogs = this.fixture.Session.Query<Blog>().ToList();
            Assert.NotEmpty(blogs);
        }

        [Fact]
        public void SimpleFetchWorks() {
            var posts = fixture.Session.Query<Post>().Fetch(p => p.Blog);
            Assert.NotNull(posts.First().Blog.Title);
        }

        [Fact]
        public void MultipleFetchParentWorks() {
            var posts = fixture.Session.Query<PostTag>().Fetch(p => p.Post).Fetch(p => p.Tag).OrderBy(p => p.PostTagId).ToList();
            Assert.NotNull(posts.First().Post.Title);
            Assert.NotNull(posts.First().Tag.Content);
        }

        [Fact]
        public void NestedFetchWorks() {
            var comment = fixture.Session.Query<Comment>().Fetch(c => c.Post.Blog).OrderBy(c => c.CommentId);
            Assert.NotNull(comment.First().Post.Blog.Title);
        }

        [Fact]
        public void MultipleFetchWithNestedWorks() {
            var comment = fixture.Session.Query<Comment>().Fetch(c => c.Post.Blog).Fetch(c => c.User).OrderBy(c => c.CommentId);
            Assert.NotNull(comment.First().Post.Blog.Title);
            Assert.NotNull(comment.First().User.Username);
        }

        [Fact]
        public void NullableFetchReturnsNull() {
            var comment = fixture.Session.Query<Comment>().Fetch(c => c.User).Where(c => c.Content == "Nullable User Content");
            Assert.Null(comment.First().User);
        }

        [Fact]
        public void NullableTripleFetchDoesNotThrow() {
            var comment = fixture.Session.Query<PostTag>().Fetch(c => c.Post.Blog).Where(t => t.Tag.Content == "Null Post Tag");
            Assert.Null(comment.First().Post);
        }

        [Fact]
        public void FetchWithNonFetchedWhere() {
            var comment = fixture.Session.Query<Comment>().Fetch(c => c.Post.Blog).Where(c => c.User.EmailAddress == "foo");
            Assert.Null(comment.FirstOrDefault());
        }
    }
}
