using Microsoft.QualityTools.Testing.Fakes;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class QueryTest : BaseQueryWriterTest
    {
        [Fact]
        public void ForUpdateSet()
        {
            Dapper.Fakes.ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType<Post>((connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
            GetTopHat().Query<Post>().Where(p => p.PostId == 1).ForUpdate().ToList();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.ForUpdate)));
        }

        [Fact]
        public void SkipSet()
        {
            Dapper.Fakes.ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType<Post>((connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
            GetTopHat().Query<Post>().Skip(10).ToList();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.Skip == 10)));
        }

        [Fact]
        public void TakeSet()
        {
            Dapper.Fakes.ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType<Post>((connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
            GetTopHat().Query<Post>().Take(10).ToList();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.Take == 10)));
        }
    }
}