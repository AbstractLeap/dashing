using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class WhereTest : BaseQueryWriterTest
    {
        [Fact]
        public void WhereExpression()
        {
            Dapper.Fakes.ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType<Post>((connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
            GetTopHat().Query<Post>().Where(p => p.PostId == 1).ToList();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.WhereClauses.Count == 1 && q.WhereClauses.First().IsExpression())));
        }

        [Fact]
        public void WhereClause()
        {
            Dapper.Fakes.ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType<Post>((connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
            GetTopHat().Query<Post>().Where("postid = 1").ToList();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.WhereClauses.Count == 1 && !q.WhereClauses.First().IsExpression() && q.WhereClauses.First().Clause == "postid = 1")));
        }

        [Fact]
        public void WhereClauseParams()
        {
            Dapper.Fakes.ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType<Post>((connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
            GetTopHat().Query<Post>().Where("postid = 1", new { PostId = 1 }).ToList();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.WhereClauses.Count == 1 && !q.WhereClauses.First().IsExpression() && q.WhereClauses.First().Clause == "postid = 1")));
        }
    }
}