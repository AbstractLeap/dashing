namespace Dashing.Tests {
  using System.ComponentModel;

  using global::Dashing.Tests.TestDomain;

  using Xunit;

  public class OrderTest : BaseQueryWriterTest {
    [Fact]
    public void OrderExpression() {
      var queryWriter = this.GetDashing().Query<Post>().OrderBy(p => p.PostId);
      Assert.True(
        queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1
        && queryWriter.Query.OrderClauses.First().Direction == ListSortDirection.Ascending && queryWriter.Query.OrderClauses.First().IsExpression());
    }

    [Fact]
    public void OrderDescendingExpression() {
      var queryWriter = this.GetDashing().Query<Post>().OrderByDescending(p => p.PostId);
      Assert.True(
        queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1
        && queryWriter.Query.OrderClauses.First().Direction == ListSortDirection.Descending && queryWriter.Query.OrderClauses.First().IsExpression());
    }

    [Fact]
    public void OrderClause() {
      var queryWriter = this.GetDashing().Query<Post>().OrderBy("blah");
      Assert.True(
        queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1
        && queryWriter.Query.OrderClauses.First().Direction == ListSortDirection.Ascending && !queryWriter.Query.OrderClauses.First().IsExpression()
        && queryWriter.Query.OrderClauses.First().Clause == "blah");
    }

    [Fact]
    public void OrderClauseDescending() {
      var queryWriter = this.GetDashing().Query<Post>().OrderByDescending("blah");
      Assert.True(
        queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1
        && queryWriter.Query.OrderClauses.First().Direction == ListSortDirection.Descending && !queryWriter.Query.OrderClauses.First().IsExpression()
        && queryWriter.Query.OrderClauses.First().Clause == "blah");
    }
  }
}