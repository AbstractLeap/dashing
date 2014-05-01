using Moq;
using System;
using System.Linq;
using TopHat.Configuration;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class OrderTest : BaseQueryWriterTest
    {
        [Fact]
        public void OrderExpression()
        {
            var queryWriter = GetTopHat().Query<Post>().OrderBy(p => p.PostId);
            Assert.True(queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1 && queryWriter.Query.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Ascending && queryWriter.Query.OrderClauses.First().IsExpression());
        }

        [Fact]
        public void OrderDescendingExpression()
        {
            var queryWriter = GetTopHat().Query<Post>().OrderByDescending(p => p.PostId);
            Assert.True(queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1 && queryWriter.Query.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Descending && queryWriter.Query.OrderClauses.First().IsExpression());
        }

        [Fact]
        public void OrderClause()
        {
            var queryWriter = GetTopHat().Query<Post>().OrderBy("blah");
            Assert.True(queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1 && queryWriter.Query.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Ascending && !queryWriter.Query.OrderClauses.First().IsExpression() && queryWriter.Query.OrderClauses.First().Clause == "blah");
        }

        [Fact]
        public void OrderClauseDescending()
        {
            var queryWriter = GetTopHat().Query<Post>().OrderByDescending("blah");
            Assert.True(queryWriter.Query.QueryType == QueryType.Select && queryWriter.Query.OrderClauses.Count == 1 && queryWriter.Query.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Descending && !queryWriter.Query.OrderClauses.First().IsExpression() && queryWriter.Query.OrderClauses.First().Clause == "blah");
        }
    }
}