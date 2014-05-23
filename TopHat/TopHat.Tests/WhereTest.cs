namespace TopHat.Tests {
  using System.Collections.Generic;

  using Dapper.Fakes;

  using Moq;

  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class WhereTest : BaseQueryWriterTest {
    [Fact]
    public void WhereExpression() {
      ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType(
        (connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
      this.GetTopHat().Query<Post>().Where(p => p.PostId == 1).ToList();
      this.SqlWriter.Verify(
        s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.WhereClauses.Count == 1 && q.WhereClauses.First().IsExpression())));
    }

    [Fact]
    public void WhereClause() {
      ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType(
        (connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
      this.GetTopHat().Query<Post>().Where("postid = 1").ToList();
      this.SqlWriter.Verify(
        s =>
        s.WriteSqlFor<Post>(
          It.Is<Query<Post>>(
            q => q.QueryType == QueryType.Select && q.WhereClauses.Count == 1 && !q.WhereClauses.First().IsExpression() && q.WhereClauses.First().Clause == "postid = 1")));
    }

    [Fact]
    public void WhereClauseParams() {
      ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType(
        (connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
      this.GetTopHat().Query<Post>().Where("postid = 1", new { PostId = 1 }).ToList();
      this.SqlWriter.Verify(
        s =>
        s.WriteSqlFor<Post>(
          It.Is<Query<Post>>(
            q => q.QueryType == QueryType.Select && q.WhereClauses.Count == 1 && !q.WhereClauses.First().IsExpression() && q.WhereClauses.First().Clause == "postid = 1")));
    }
  }
}