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

            // @formatter:off
            Assert.Single(rootNode.IncludedColumns);
            Assert.Equal(nameof(Post.Title), rootNode.IncludedColumns.Single().Name);
            // @formatter:on
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

            // @formatter:off
            Assert.Equal(2, rootNode.IncludedColumns.Count);
            Assert.Equal(nameof(Post.Title), rootNode.IncludedColumns.ElementAt(0).Name);
            Assert.Equal(nameof(Post.PostId), rootNode.IncludedColumns.ElementAt(1).Name);
            // @formatter:on
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

            // @formatter:off
            Assert.Single(rootNode.Children);
            Assert.Single(rootNode.IncludedColumns);
            Assert.True(rootNode.Children[nameof(Post.Blog)].IsFetched);
            // @formatter:on
        }

        [Fact]
        public void SelectParentWorks()
        {
            var parser = GetParser<Post>();
            var rootNode = new FetchNode();
            parser.ParseExpression(
                p => new
                     {
                         Title = p.Title,
                         BlogTitle = p.Blog.Title
                     },
                rootNode);

            // @formatter:off
            Assert.Single(rootNode.IncludedColumns);
            Assert.Equal(nameof(Post.Title), rootNode.IncludedColumns.Single().Name);
            Assert.Single(rootNode.Children);
            Assert.Single(rootNode.Children[nameof(Post.Blog)].IncludedColumns);
            Assert.Equal(nameof(Blog.Title), rootNode.Children[nameof(Post.Blog)].IncludedColumns.Single().Name);
            // @formatter:on
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