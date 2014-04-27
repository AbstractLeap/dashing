using Moq;
using System;
using System.Linq;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class OrderTest : BaseQueryWriterTest
    {
        [Fact]
        public void OrderExpression()
        {
            GetTopHat().Query<Post>().OrderBy(p => p.PostId);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.OrderClauses.Count == 1 && q.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Ascending && q.OrderClauses.First().IsExpression())));
        }

        [Fact]
        public void OrderDescendingExpression()
        {
            GetTopHat().Query<Post>().OrderByDescending(p => p.PostId);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.OrderClauses.Count == 1 && q.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Descending && q.OrderClauses.First().IsExpression())));
        }

        [Fact]
        public void OrderClause()
        {
            GetTopHat().Query<Post>().OrderBy("blah");
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.OrderClauses.Count == 1 && q.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Ascending && !q.OrderClauses.First().IsExpression() && q.OrderClauses.First().Clause == "blah")));
        }

        [Fact]
        public void OrderClauseDescending()
        {
            GetTopHat().Query<Post>().OrderByDescending("blah");
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.OrderClauses.Count == 1 && q.OrderClauses.First().Direction == System.ComponentModel.ListSortDirection.Descending && !q.OrderClauses.First().IsExpression() && q.OrderClauses.First().Clause == "blah")));
        }
    }
}