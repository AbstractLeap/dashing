namespace Dashing.Tests.Engine.DML {
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.OneToOne;

    using Xunit;

    public class OrderClauseWriterTests {
        [Fact]
        public void OrderByPrimaryKeyContainsPrimaryKeyClause() {
            Expression<Func<Post, int>> cls = p => p.PostId;
            var clause = new OrderClause<Post>(cls, ListSortDirection.Ascending);
            var writer = new OrderClauseWriter(new CustomConfig(), new SqlServerDialect());
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(clause, null, out containsPrimaryKeyClause);
            Assert.True(containsPrimaryKeyClause);
        }

        [Fact]
        public void SimpleClauseCorrect() {
            Expression<Func<Post, string>> cls = p => p.Title;
            var clause = new OrderClause<Post>(cls, ListSortDirection.Ascending);
            var writer = new OrderClauseWriter(new CustomConfig(), new SqlServerDialect());
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(clause, null, out containsPrimaryKeyClause);
            Assert.Equal("[Title] asc", result);
        }

        [Fact]
        public void SimpleClauseDescendingCorrect() {
            Expression<Func<Post, string>> cls = p => p.Title;
            var clause = new OrderClause<Post>(cls, ListSortDirection.Descending);
            var writer = new OrderClauseWriter(new CustomConfig(), new SqlServerDialect());
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(clause, null, out containsPrimaryKeyClause);
            Assert.Equal("[Title] desc", result);
        }

        [Fact]
        public void OrderByForeignKeyAsc() {
            Expression<Func<Post, Blog>> cls = p => p.Blog;
            var clause = new OrderClause<Post>(cls, ListSortDirection.Ascending);
            var writer = new OrderClauseWriter(new CustomConfig(), new SqlServerDialect());
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(clause, null, out containsPrimaryKeyClause);
            Assert.Equal("[BlogId] asc", result);
        }

        [Fact]
        public void OrderByForeignKeyDesc() {
            Expression<Func<Post, Blog>> cls = p => p.Blog;
            var clause = new OrderClause<Post>(cls, ListSortDirection.Descending);
            var writer = new OrderClauseWriter(new CustomConfig(), new SqlServerDialect());
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(clause, null, out containsPrimaryKeyClause);
            Assert.Equal("[BlogId] desc", result);
        }

        [Fact]
        public void OrderByNestedForeignKeyAsc() {
            var query = new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Blog).OrderBy(p => p.Blog.CreateDate);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Post>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Post>)query), out containsPrimaryKeyClause);
            Assert.Equal("t_1.[CreateDate] asc", result);
        }

        [Fact]
        public void OrderByNestedForeignKeyDesc() {
            var query = new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Blog).OrderByDescending(p => p.Blog.CreateDate);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Post>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Post>)query), out containsPrimaryKeyClause);
            Assert.Equal("t_1.[CreateDate] desc", result);
        }

        [Fact]
        public void OrderByWithFetchTreeAsc() {
            var query = new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Blog).OrderBy(p => p.Title);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Post>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Post>)query), out containsPrimaryKeyClause);
            Assert.Equal("t.[Title] asc", result);
        }

        [Fact]
        public void OrderByWithFetchTreeDesc() {
            var query = new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Blog).OrderByDescending(p => p.Title);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Post>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Post>)query), out containsPrimaryKeyClause);
            Assert.Equal("t.[Title] desc", result);
        }

        [Fact]
        public void OrderByNestedNestedForeignKeyAsc() {
            var query = new SelectQuery<Comment>(new NonExecutingSelectQueryExecutor()).Fetch(c => c.Post.Blog).OrderBy(c => c.Post.Blog);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Comment>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Comment>)query), out containsPrimaryKeyClause);
            Assert.Equal("t_1.[BlogId] asc", result);
        }

        [Fact]
        public void OrderByNestedNestedForeignKeyDesc() {
            var query = new SelectQuery<Comment>(new NonExecutingSelectQueryExecutor()).Fetch(c => c.Post.Blog).OrderByDescending(c => c.Post.Blog);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Comment>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Comment>)query), out containsPrimaryKeyClause);
            Assert.Equal("t_1.[BlogId] desc", result);
        }

        [Fact]
        public void OrderByNestedNestedPropAsc() {
            var query = new SelectQuery<Comment>(new NonExecutingSelectQueryExecutor()).Fetch(c => c.Post.Blog).OrderBy(c => c.Post.Blog.Title);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Comment>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Comment>)query), out containsPrimaryKeyClause);
            Assert.Equal("t_2.[Title] asc", result);
        }

        [Fact]
        public void OrderByNestedNestedPropDesc() {
            var query = new SelectQuery<Comment>(new NonExecutingSelectQueryExecutor()).Fetch(c => c.Post.Blog).OrderByDescending(c => c.Post.Blog.Title);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(((SelectQuery<Comment>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Comment>)query), out containsPrimaryKeyClause);
            Assert.Equal("t_2.[Title] desc", result);
        }

        [Fact]
        public void OrderByNestedNotFetchedThrows() {
            var query = new SelectQuery<Comment>(new NonExecutingSelectQueryExecutor()).Fetch(c => c.Post).OrderByDescending(c => c.Post.Blog.Title);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            Assert.Throws<InvalidOperationException>(() => writer.GetOrderClause(((SelectQuery<Comment>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Comment>)query), out containsPrimaryKeyClause));
        }

        [Fact]
        public void OrderByNotFetchedThrows() {
            var query = new SelectQuery<Comment>(new NonExecutingSelectQueryExecutor()).OrderByDescending(c => c.Post.Title);
            var config = new CustomConfig();
            var dialect = new SqlServerDialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            bool containsPrimaryKeyClause = false;
            Assert.Throws<InvalidOperationException>(() => writer.GetOrderClause(((SelectQuery<Comment>)query).OrderClauses.Dequeue(), fetchTreeWriter.GetFetchTree((SelectQuery<Comment>)query), out containsPrimaryKeyClause));
        }

        [Fact]
        public void OrderAcrossOneToOneWorks() {
            var query = new SelectQuery<OneToOneLeft>(new NonExecutingSelectQueryExecutor()).Fetch(o => o.Right).OrderBy(o => o.Right.Name);
            var config = new OneToOneConfig();
            var dialect = new SqlServer2012Dialect();
            var writer = new OrderClauseWriter(config, dialect);
            var fetchTreeWriter = new FetchTreeWriter(dialect, config);
            var containsPrimaryKeyClause = false;
            var result = writer.GetOrderClause(
                ((SelectQuery<OneToOneLeft>)query).OrderClauses.Dequeue(),
                fetchTreeWriter.GetFetchTree((SelectQuery<OneToOneLeft>)query),
                out containsPrimaryKeyClause);
            Assert.Equal("t_1.[Name] asc", result);
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }

        private class FetchTreeWriter : SelectWriter {
            public FetchTreeWriter(ISqlDialect dialect, IConfiguration config)
                : base(dialect, config) {
            }

            public FetchNode GetFetchTree<T>(SelectQuery<T> selectQuery) {
                int aliasCounter;
                int numberCollectionFetches;
                return base.GetFetchTree(selectQuery, out aliasCounter, out numberCollectionFetches);
            }
        }

        private class OneToOneConfig : MockConfiguration {
            public OneToOneConfig() {
                this.AddNamespaceOf<OneToOneLeft>();
            }
        }
    }
}