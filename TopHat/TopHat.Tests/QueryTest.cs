namespace TopHat.Tests {
  using System.Collections.Generic;

  using Dapper.Fakes;

  using Moq;

  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class QueryTest : BaseQueryWriterTest {
    [Fact]
    public void ForUpdateSet() {
      ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType(
        (connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
      this.GetTopHat().Query<Post>().Where(p => p.PostId == 1).ForUpdate().ToList();
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.ForUpdate)));
    }

    [Fact]
    public void SkipSet() {
      ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType(
        (connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
      this.GetTopHat().Query<Post>().Skip(10).ToList();
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.Skip == 10)));
    }

    [Fact]
    public void TakeSet() {
      ShimSqlMapper.QueryOf1IDbConnectionStringObjectIDbTransactionBooleanNullableOfInt32NullableOfCommandType(
        (connection, sql, parameters, transaction, buffered, timeout, type) => new List<Post>());
      this.GetTopHat().Query<Post>().Take(10).ToList();
      this.SqlWriter.Verify(s => s.WriteSqlFor<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.Take == 10)));
    }
  }
}