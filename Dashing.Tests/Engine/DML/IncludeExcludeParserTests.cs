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
            var parser = new IncludeExcludeParser(this.GetConfig<Post, string>(post => post.Title));
            Expression<Func<Post, string>> pred = post => post.Title;
            var fetchNode = new FetchNode();
            parser.ParseExpression<Post>(pred, fetchNode, true);

            Assert.Single(fetchNode.IncludedColumns);
            Assert.Null(fetchNode.ExcludedColumns);
            Assert.Equal(nameof(Post.Title), fetchNode.IncludedColumns.First().Name);
        }

        [Fact]
        public void NonFetchedIncludeThrows() {
            var parser = new IncludeExcludeParser(this.GetConfig<Blog, string>(blog => blog.Description));
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var fetchNode = new FetchNode();
            Assert.Throws<InvalidOperationException>(() => parser.ParseExpression<Post>(pred, fetchNode, true));
        }

        [Fact]
        public void RelationshipIncludeThrows() {
            var parser = new IncludeExcludeParser(this.GetConfig<Blog, string>(blog => blog.Description));
            Expression<Func<Post, Blog>> pred = post => post.Blog;
            var fetchNode = new FetchNode();
            Assert.Throws<NotSupportedException>(() => parser.ParseExpression<Post>(pred, fetchNode, true));
        }

        [Fact]
        public void FetchedIncludeWorks() {
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            var parser = new IncludeExcludeParser(config);
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var rootNode = new FetchNode();
            var blogNode = rootNode.AddChild(config.GetMap<Post>().Property(p => p.Blog), true);
            rootNode.Children = new OrderedDictionary<string, FetchNode> {
                                                                             { nameof(Post.Blog), blogNode }
                                                                         };
            parser.ParseExpression<Post>(pred, rootNode, true);

            Assert.Null(rootNode.IncludedColumns);
            Assert.Null(rootNode.ExcludedColumns);
            Assert.Single(blogNode.IncludedColumns);
            Assert.Null(blogNode.ExcludedColumns);
            Assert.Equal(nameof(Blog.Description), blogNode.IncludedColumns.First().Name);
        }

        [Fact]
        public void SimpleExcludeAddedToTree() {
            var parser = new IncludeExcludeParser(this.GetConfig<Post, string>(post => post.Title));
            Expression<Func<Post, string>> pred = post => post.Title;
            var fetchNode = new FetchNode();
            parser.ParseExpression<Post>(pred, fetchNode, false);

            Assert.Single(fetchNode.ExcludedColumns);
            Assert.Null(fetchNode.IncludedColumns);
            Assert.Equal(nameof(Post.Title), fetchNode.ExcludedColumns.First().Name);
        }

        [Fact]
        public void NonFetchedExcludeThrows() {
            var parser = new IncludeExcludeParser(this.GetConfig<Blog, string>(blog => blog.Description));
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var fetchNode = new FetchNode();
            Assert.Throws<InvalidOperationException>(() => parser.ParseExpression<Post>(pred, fetchNode, false));
        }

        [Fact]
        public void RelationshipExcludeThrows() {
            var parser = new IncludeExcludeParser(this.GetConfig<Blog, string>(blog => blog.Description));
            Expression<Func<Post, Blog>> pred = post => post.Blog;
            var fetchNode = new FetchNode();
            Assert.Throws<NotSupportedException>(() => parser.ParseExpression<Post>(pred, fetchNode, false));
        }

        [Fact]
        public void FetchedExcludeWorks() {
            var config = this.GetConfig<Blog, string>(blog => blog.Description);
            var parser = new IncludeExcludeParser(config);
            Expression<Func<Post, string>> pred = post => post.Blog.Description;
            var rootNode = new FetchNode();
            var blogNode = rootNode.AddChild(config.GetMap<Post>().Property(p => p.Blog), true);
            rootNode.Children = new OrderedDictionary<string, FetchNode> {
                                                                             { nameof(Post.Blog), blogNode }
                                                                         };
            parser.ParseExpression<Post>(pred, rootNode, false);

            Assert.Null(rootNode.IncludedColumns);
            Assert.Null(rootNode.ExcludedColumns);
            Assert.Single(blogNode.ExcludedColumns);
            Assert.Null(blogNode.IncludedColumns);
            Assert.Equal(nameof(Blog.Description), blogNode.ExcludedColumns.First().Name);
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