namespace Dashing.Tests {
    using System.Data;

    using Dashing.Engine;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class SelectQueryTests {
        private readonly Mock<IDbConnection> connection;

        private readonly Mock<IDbTransaction> transaction;

        private readonly Mock<IEngine> mockEngine;

        public SelectQueryTests() {
            this.connection = new Mock<IDbConnection>(MockBehavior.Strict);
            this.connection.Setup(c => c.State).Returns(ConnectionState.Open);
            this.transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            this.mockEngine = new Mock<IEngine>();
        }

        private ISession GetDashing() {
            return new Session(this.mockEngine.Object, this.connection.Object, transaction: this.transaction.Object);
        }

        [Fact]
        public void TestProjection() {
            var dashing = this.GetDashing();
            var query = dashing.Query<Blog>().Select(b => new {
                b.BlogId, 
                b.Title
            });

            var selectQuery = query as SelectQuery<Blog>;
            Assert.NotNull(selectQuery);
            Assert.NotNull(selectQuery.Projection);
        }

        [Fact]
        public void FetchAllProperties() {
            var query = this.GetDashing().Query<Blog>().IncludeAll();

            var selectQuery = query as SelectQuery<Blog>;
            Assert.NotNull(selectQuery);
            Assert.True(selectQuery.FetchAllProperties);
        }

        [Fact]
        public void IncludeWorks() {
            var query = this.GetDashing().Query<Post>().Include(p => p.Content);

            var selectQuery = query as SelectQuery<Post>;
            Assert.NotNull(selectQuery);
            Assert.NotEmpty(selectQuery.Includes);
        }

        [Fact]
        public void ExcludeWorks() {
            var query = this.GetDashing().Query<Post>().Exclude(p => p.Content);

            var selectQuery = query as SelectQuery<Post>;
            Assert.NotNull(selectQuery);
            Assert.NotEmpty(selectQuery.Excludes);
        }

        [Fact]
        public void SingleLevelFetch() {
            var dashing = this.GetDashing();
            var query = dashing.Query<Post>().Fetch(p => p.Blog);

            var selectQuery = query as SelectQuery<Post>;
            Assert.NotNull(selectQuery);
            Assert.NotEmpty(selectQuery.Fetches);
        }

        [Fact]
        public void MultipleLevelFetch() {
            var dashing = this.GetDashing();
            var query = dashing.Query<Comment>().Fetch(c => c.Post.Blog);

            var selectQuery = query as SelectQuery<Comment>;
            Assert.NotNull(selectQuery);
            Assert.NotEmpty(selectQuery.Fetches);
        }

        [Fact]
        public void CollectionFetch() {
            var dashing = this.GetDashing();
            var query = dashing.Query<Post>().Fetch(p => p.Comments);

            var selectQuery = query as SelectQuery<Post>;
            Assert.NotNull(selectQuery);
            Assert.NotEmpty(selectQuery.Fetches);
        }

        [Fact]
        public void CollectionParentFetch() {
            var dashing = this.GetDashing();
            var query = dashing.Query<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User);

            var selectQuery = query as SelectQuery<Post>;
            Assert.NotNull(selectQuery);
            Assert.NotNull(selectQuery.CollectionFetches);
        }
    }
}