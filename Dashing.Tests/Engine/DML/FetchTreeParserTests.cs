namespace Dashing.Tests.Engine.DML {
    using System.Linq;

    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class FetchTreeParserTests {
        [Fact]
        public void FetchReturnsFetchNode() {
            var config = new TestConfig();
            var parser = new FetchTreeParser(config);
            var query = GetSelectQuery<Post>()
                .Fetch(p => p.Blog) as SelectQuery<Post>;
            var queryTree = parser.GetFetchTree(query, out var _, out var _);
            Assert.NotNull(queryTree);
            Assert.Single(queryTree.Children);
            Assert.Equal(nameof(Post.Blog), queryTree.Children.Keys.First());
            Assert.Same(config.GetMap<Post>().Columns[nameof(Post.Blog)], queryTree.Children.Values.First().Column);
            Assert.True(queryTree.Children.Values.First().IsFetched);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new()
        {
            return new SelectQuery<T>(new Mock<IProjectedSelectQueryExecutor>().Object);
        }
    }
}