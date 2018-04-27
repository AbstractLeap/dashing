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

namespace Dashing.Tests.Engine.DML
{
    public class SqlSelectWriterTests
    {
        [Fact]
        public void ItWorks()
        {
            var sqlBuilder = new Dashing.SqlBuilder.SqlBuilder(null);
            var from = sqlBuilder.From<TestDomain.Post>().InnerJoin<TestDomain.User>();
            var select = from.Select((p, u) => p.Title);
            var sqlSelectWriter = new SqlSelectWriter(new SqlServer2012Dialect(), new TestConfig());
            var output = sqlSelectWriter.GenerateSql((SqlFromDefinition<TestDomain.Post, TestDomain.User>)from, (Expression<Func<TestDomain.Post, TestDomain.User, string>>)((p, u) => p.Title));
            Assert.Equal("", output.Sql);
        }
    }
}
