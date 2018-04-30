namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.SqlBuilder;
    using Dashing.Tests.TestDomain;

    using Xunit;
    using Xunit.Abstractions;

    public class SqlSelectWriterTests {
        private readonly ITestOutputHelper output;

        public SqlSelectWriterTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void ItWorks() {
            this.output.WriteLine($"Debugger: {(Debugger.IsAttached ? "Attached" : "Not attached")}");
            var sqlBuilder = new SqlBuilder(null);
            var from = sqlBuilder.From<Post>()
                                 .InnerJoin<User>((p, u) => p.Author == u)
                                 .Where((post, user) => post.Blog.BlogId == 1);
            var select = from.Select((p, u) => p.Title);
            var sqlSelectWriter = new SqlSelectWriter(new SqlServer2012Dialect(), new TestConfig());
            var output = sqlSelectWriter.GenerateSql((SqlFromDefinition<Post, User>)from, (Expression<Func<Post, User, string>>)((p, u) => p.Title));
            this.output.WriteLine(output.Sql);
            Assert.Equal("", output.Sql);
        }
    }
}