namespace Dashing.Tests.Engine.DML {
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class SelectProjectionParserTests {
        [Fact]
        public void SelectBaseWorks() {
            var targets = GetParser<Post>();
            var rootNode = new QueryTree(true, false, targets.Config.GetMap<Post>());
            targets.Parser.ParseExpression(
                p => new Post {
                                  Title = p.Title
                              },
                rootNode);

            // @formatter:off
            Assert.Single(rootNode.GetSelectedColumns());
            Assert.Equal(nameof(Post.Title), rootNode.GetSelectedColumns().First().Name);
            // @formatter:on
        }

        [Fact]
        public void MultipleBaseWorks()
        {
            var targets = GetParser<Post>();
            var rootNode = new QueryTree(true, false, targets.Config.GetMap<Post>());
            targets.Parser.ParseExpression(
                p => new Post {
                                  Title = p.Title,
                                  PostId = p.PostId
                              },
                rootNode);

            // @formatter:off
            Assert.Equal(2, rootNode.GetSelectedColumns().Count());
            Assert.Equal(nameof(Post.Title), rootNode.GetSelectedColumns().ElementAt(0).Name);
            Assert.Equal(nameof(Post.PostId), rootNode.GetSelectedColumns().ElementAt(1).Name);
            // @formatter:on
        }

        [Fact]
        public void IncludedManyToOneRelationshipWorks()
        {
            var targets = GetParser<Post>();
            var rootNode = new QueryTree(true, false, targets.Config.GetMap<Post>());
            targets.Parser.ParseExpression(
                p => new Post {
                                  Title = p.Title,
                                  Blog = p.Blog
                              },
                rootNode);

            // @formatter:off
            Assert.Single(rootNode.Children);
            Assert.Single(rootNode.GetSelectedColumns());
            Assert.True(rootNode.Children[nameof(Post.Blog)].IsFetched);
            // @formatter:on
        }

        [Fact]
        public void SelectParentWorks()
        {
            var targets = GetParser<Post>();
            var rootNode = new QueryTree(true, false, targets.Config.GetMap<Post>());
            targets.Parser.ParseExpression(
                p => new
                     {
                         Title = p.Title,
                         BlogTitle = p.Blog.Title
                     },
                rootNode);

            // @formatter:off
            Assert.Single(rootNode.GetSelectedColumns());
            Assert.Equal(nameof(Post.Title), rootNode.GetSelectedColumns().Single().Name);
            Assert.Single(rootNode.Children);
            Assert.Single(rootNode.Children[nameof(Post.Blog)].GetSelectedColumns());
            Assert.Equal(nameof(Blog.Title), rootNode.Children[nameof(Post.Blog)].GetSelectedColumns().Single().Name);
            // @formatter:on
        }

        private static (IConfiguration Config, SelectProjectionParser<T> Parser) GetParser<T>() {
            var config = new CustomConfig();
            var parser = new SelectProjectionParser<T>(config);
            return (config, parser);
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}