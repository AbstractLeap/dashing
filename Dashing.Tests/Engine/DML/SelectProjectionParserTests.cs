namespace Dashing.Tests.Engine.DML {
    using System.Linq;

    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class SelectProjectionParserTests {
        [Fact]
        public void SelectBaseWorks() {
            var parser = GetParser<Post>();
            var rootNode = new FetchNode();
            parser.ParseExpression(
                p => new Post {
                                  Title = p.Title
                              },
                rootNode);

            Assert.Single(rootNode.IncludedColumns);
            Assert.Equal(
                nameof(Post.Title),
                rootNode.IncludedColumns.Single()
                        .Name);
        }

        [Fact]
        public void MultipleBaseWorks() {
            var parser = GetParser<Post>();
            var rootNode = new FetchNode();
            parser.ParseExpression(
                p => new Post {
                                  Title = p.Title,
                                  PostId = p.PostId
                              },
                rootNode);

            Assert.Equal(2, rootNode.IncludedColumns.Count);
            Assert.Equal(
                nameof(Post.Title),
                rootNode.IncludedColumns.ElementAt(0)
                        .Name);
            Assert.Equal(
                nameof(Post.PostId),
                rootNode.IncludedColumns.ElementAt(1)
                        .Name);
        }

        [Fact]
        public void IncludedManyToOneRelationshipWorks() {
            var parser = GetParser<Post>();
            var rootNode = new FetchNode();
            parser.ParseExpression(
                p => new Post {
                                  Title = p.Title,
                                  Blog = p.Blog
                              },
                rootNode);

            Assert.Single(rootNode.Children);
            Assert.Single(rootNode.IncludedColumns);
            Assert.True(
                rootNode.Children[nameof(Post.Blog)]
                        .IsFetched);
        }

        private static SelectProjectionParser<T> GetParser<T>() {
            var parser = new SelectProjectionParser<T>(new CustomConfig());
            return parser;
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}