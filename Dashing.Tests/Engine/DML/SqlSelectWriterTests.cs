using Dashing.Engine.Dialects;
using Dashing.Engine.DML;
using Dashing.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Dashing.Tests.Engine.DML
{
    using System.Diagnostics;

    public class SqlSelectWriterTests
    {
        private readonly ITestOutputHelper output;

        public SqlSelectWriterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ItWorks()
        {
            this.output.WriteLine($"Debugger: {(Debugger.IsAttached ? "Attached" : "Not attached")}");
            var sqlBuilder = new Dashing.SqlBuilder.SqlBuilder(null);
            var from = sqlBuilder
                .From<TestDomain.Post>()
                .InnerJoin<TestDomain.User>((p, u) =>  p.Author == u)
                .Where((post, user) => post.Blog.BlogId == 1);
            var select = from.Select((p, u) => p.Title);
            var sqlSelectWriter = new SqlSelectWriter(new SqlServer2012Dialect(), new TestConfig());
            var output = sqlSelectWriter.GenerateSql((SqlFromDefinition<TestDomain.Post, TestDomain.User>)from, (Expression<Func<TestDomain.Post, TestDomain.User, string>>)((p, u) => p.Title));
            this.output.WriteLine(output.Sql);
            Assert.Equal("", output.Sql);
        }
    }
}
