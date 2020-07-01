namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.DML;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class IncludeExcludeParserTests {
        [Fact]
        public void SimpleIncludeAddedToTree() {
            Expression<Func<Post, string>> pred = post => post.Title;
            var config = this.GetConfig<Post, string>(post => post.Title);
            var fetchNode = this.Parse(config, pred);

            Assert.Contains(fetchNode.GetSelectedColumns(), column => column.Name == nameof(Post.Title));
        }

        [Fact]
        public void NonFetchedIncludeThrows() {
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            Assert.Throws<InvalidOperationException>(() => this.Parse(config, pred));
        }

        [Fact]
        public void RelationshipIncludeThrows() {
            Expression<Func<Post, Blog>> pred = post => post.Blog;
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            Assert.Throws<NotSupportedException>(() => this.Parse(config, pred));
        }

        [Fact]
        public void FetchedIncludeWorks() {
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            var rootNode = new QueryTree(false, false, config.GetMap<Post>());
            var blogNode = rootNode.AddChild(config.GetMap<Post>().Property(p => p.Blog), true);
            rootNode.Children = new OrderedDictionary<string, QueryNode> {
                                                                             { nameof(Post.Blog), blogNode }
                                                                         };
            rootNode = this.Parse(config, pred, rootNode);

            Assert.Contains(rootNode.Children[nameof(Post.Blog)].GetSelectedColumns(), c => c.Name == nameof(Blog.Description));
        }

        [Fact]
        public void SimpleExcludeAddedToTree() {
            var config = this.GetConfig<Post, string>(post => post.Title);
            Expression<Func<Post, string>> pred = post => post.Title;
            var fetchNode = this.Parse(config, pred, isInclude: false);

            Assert.True(fetchNode.GetSelectedColumns().All(c => c.Name != nameof(Post.Title)));
        }

        [Fact]
        public void NonFetchedExcludeThrows() {
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            Assert.Throws<InvalidOperationException>(() => this.Parse(config, pred, isInclude: false));
        }

        [Fact]
        public void RelationshipExcludeThrows() {
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            Expression<Func<Post, Blog>> pred = post => post.Blog;
            Assert.Throws<NotSupportedException>(() => this.Parse(config, pred, isInclude: false));
        }

        [Fact]
        public void FetchedExcludeWorks() {
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var rootNode = new QueryTree(false, false, config.GetMap<Post>());
            var blogNode = rootNode.AddChild(config.GetMap<Post>().Property(p => p.Blog), true);
            rootNode.Children = new OrderedDictionary<string, QueryNode> {
                                                                             { nameof(Post.Blog), blogNode }
                                                                         };
            rootNode = this.Parse(config, pred, rootNode, isInclude: false);

            Assert.True(rootNode.Children.First().Value.GetSelectedColumns().All(c => c.Name != nameof(Blog.Description)));
        }

        private QueryTree Parse<T, TInclude>(IConfiguration configuration, Expression<Func<T, TInclude>> pred, QueryTree queryTree = null, bool isInclude = true)
        {
            var parser = new IncludeExcludeParser(configuration);
            if (queryTree == null) {
                queryTree = new QueryTree(false, false, configuration.GetMap<T>());
            }

            parser.ParseExpression<Post>(pred, queryTree, isInclude);
            return queryTree;
        }

        private IConfiguration GetConfig() {
            return this.GetConfig<string, string>(null);
        }

        private IConfiguration GetConfig<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> excludeByDefault = null) {
            var config = new MutableConfiguration();
            config.AddNamespaceOf<Post>();
            if (excludeByDefault != null) {
                config.Setup<TEntity>()
                      .Property(excludeByDefault)
                      .ExcludeByDefault();
            }

            return config;
        }
    }
}