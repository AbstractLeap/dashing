namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.DML;
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
            Assert.Equal(nameof(Post.Title), fetchNode.IncludedColumns.First().Name);
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