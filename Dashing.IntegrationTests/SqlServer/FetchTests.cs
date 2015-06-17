namespace Dashing.IntegrationTests.SqlServer {
    using System.Linq;

    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.IntegrationTests.TestDomain.More.MultipleFetchManyWithNonRootAndThenFetchDomain;

    using Xunit;

    public class FetchTests : IClassFixture<SqlServerFixture>, IClassFixture<MultipleFetchManyWithNonRootAndThenFetchSqlServerFixture> {
        private readonly SqlServerFixture fixture;

        private readonly MultipleFetchManyWithNonRootAndThenFetchSqlServerFixture multipleFetchManyWithNonRootAndThenFetchSqlServerFixture;

        public FetchTests(SqlServerFixture data, MultipleFetchManyWithNonRootAndThenFetchSqlServerFixture multipleFetchManyWithNonRootAndThenFetchSqlServerFixture) {
            this.fixture = data;
            this.multipleFetchManyWithNonRootAndThenFetchSqlServerFixture = multipleFetchManyWithNonRootAndThenFetchSqlServerFixture;
        }

        [Fact]
        public void ExecuteSimpleQuery() {
            var blogs = this.fixture.Session.Query<Blog>().ToList();
            Assert.NotEmpty(blogs);
        }

        [Fact]
        public void SimpleFetchWorks() {
            var posts = this.fixture.Session.Query<Post>().Fetch(p => p.Blog);
            Assert.NotNull(posts.First().Blog.Title);
        }

        [Fact]
        public void MultipleFetchParentWorks() {
            var posts = this.fixture.Session.Query<PostTag>().Fetch(p => p.Post).Fetch(p => p.Tag).OrderBy(p => p.PostTagId).ToList();
            Assert.NotNull(posts.First().Post.Title);
            Assert.NotNull(posts.First().Tag.Content);
        }

        [Fact]
        public void NestedFetchWorks() {
            var comment = this.fixture.Session.Query<Comment>().Fetch(c => c.Post.Blog).OrderBy(c => c.CommentId);
            Assert.NotNull(comment.First().Post.Blog.Title);
        }

        [Fact]
        public void MultipleFetchWithNestedWorks() {
            var comment = this.fixture.Session.Query<Comment>().Fetch(c => c.Post.Blog).Fetch(c => c.User).OrderBy(c => c.CommentId);
            Assert.NotNull(comment.First().Post.Blog.Title);
            Assert.NotNull(comment.First().User.Username);
        }

        [Fact]
        public void NullableFetchReturnsNull() {
            var comment = this.fixture.Session.Query<Comment>().Fetch(c => c.User).Where(c => c.Content == "Nullable User Content");
            Assert.Null(comment.First().User);
        }

        [Fact]
        public void NullableTripleFetchDoesNotThrow() {
            var comment = this.fixture.Session.Query<PostTag>().Fetch(c => c.Post.Blog).Where(t => t.Tag.Content == "Null Post Tag");
            Assert.Null(comment.First().Post);
        }

        [Fact]
        public void FetchWithNonFetchedWhere() {
            var comment = this.fixture.Session.Query<Comment>().Fetch(c => c.Post.Blog).Where(c => c.User.EmailAddress == "foo");
            Assert.Null(comment.FirstOrDefault());
        }

        [Fact]
        public void MultipleFetchManyWithNonRootAndThenFetchWorks() {
            var responses =
                this.multipleFetchManyWithNonRootAndThenFetchSqlServerFixture.Session.Query<QuestionnaireResponse>()
                    .Where(qr => qr.Questionnaire.QuestionnaireId == 1)
                    .Fetch(qr => qr.Questionnaire)
                    .FetchMany(qr => qr.Responses)
                    .ThenFetch(qrr => qrr.Question)
                    .FetchMany(qr => qr.Booking.Beds)
                    .ThenFetch(b => b.RoomSlot.Room)
                    .ToArray();
            Assert.Equal(1, responses.Length);
            Assert.Equal(1, responses.First().Booking.Beds.Count);
        }
    }
}