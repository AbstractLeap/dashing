namespace Dashing.Tests.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dashing.Engine.Dialects;

    using Moq;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class SelectWriterTests {
        [Fact]
        public void SimpleQueryBuilds() {
            var dialect = new SqlServerDialect();
            var engine = new EngineBase(dialect, new Mock<System.Data.Common.DbProviderFactory>().Object);
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            var selectWriter = new SelectWriter(dialect, MakeConfig());
            var sql = selectWriter.GenerateSql(new SelectQuery<User>(engine, connection.Object));
            Debug.Write(sql.Sql);
        }

        [Fact]
        public void FetchTest() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId",
                sql.Sql);
        }

        [Fact]
        public void WhereId() {
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereClosureConstantAccess() {
            var o = new Post { PostId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == o.PostId);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereEntityEquals()
        {
            var o = new Post { PostId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p == o);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereAssociatedEntityEquals()
        {
            var blog = new Blog { BlogId = 1 };
            var query = this.GetSelectQuery<Post>().Where(p => p.Blog == blog);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([BlogId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereAssociatedEntityEqualsAssociatedEntity()
        {
            var o = new Post { PostId = 1, Blog = new Blog { BlogId = 1 } };
            var query = this.GetSelectQuery<Post>().Where(p => p.Blog == o.Blog);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([BlogId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void NonFetchedRelationship()
        {
            var query = this.GetSelectQuery<Post>().Where(p => p.Blog.Title == "Boo");
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] as t left join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] = @l_1)", sql.Sql);
        }

        private class Foo {
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. We want to test all cases, not just best practice.")]
            public int Id = 1;
        }

        [Fact]
        public void WhereNonMappedClosureConstantAccess() {
            var foo = new Foo();
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == foo.Id);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void WhereNonPKFetch() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author).Where(p => p.Author.Username == "bob");
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t_1.[Username] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void WherePKFetch() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author).Where(p => p.Author.UserId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t.[AuthorId] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void MultipleLevelPKFetch() {
            var query = this.GetSelectQuery<Comment>().Fetch(p => p.Post.Author).Where(p => p.Post.Author.UserId == 1);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[BlogId], t_1.[DoNotMap], t_2.[UserId], t_2.[Username], t_2.[EmailAddress], t_2.[Password], t_2.[IsEnabled], t_2.[HeightInMeters] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Users] as t_2 on t_1.AuthorId = t_2.UserId where (t_1.[AuthorId] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void WhereFetchMultiple() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Author).Where(c => c.Post.Author.Username == "bob");
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[BlogId], t_1.[DoNotMap], t_2.[UserId], t_2.[Username], t_2.[EmailAddress], t_2.[Password], t_2.[IsEnabled], t_2.[HeightInMeters] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Users] as t_2 on t_1.AuthorId = t_2.UserId where (t_2.[Username] = @l_1)",
                sql.Sql);
        }

        [Fact]
        public void FetchWithNonFetchPKWhereClause() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post).Where(c => c.User.UserId == 2);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId where (t.[UserId] = @l_1)", sql.Sql);
        }

        [Fact]
        public void FetchWithNonFetchWhereClause() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post).Where(c => c.User.EmailAddress == "blah");
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId left join [Users] as t_100 on t.UserId = t_100.UserId where (t_100.[EmailAddress] = @l_1)", sql.Sql);
        }

        [Fact]
        public void SimpleOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Rating);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Rating] asc", sql.Sql);
        }

        [Fact]
        public void WhereIdGetsGoodParams() {
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Assert.Equal(1, sql.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void IgnoreNotReturned() {
            var query = this.GetSelectQuery<Post>();
            var selectQuery = query;
            var sql = this.GetWriter(true).GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId] from [Posts]", sql.Sql);
        }

        [Fact]
        public void IgnoreNotReturnedOnFetch() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post);
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter(true).GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal(
                "select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId] from [Comments] as t left join [Posts] as t_1 on t.PostId = t_1.PostId",
                sql.Sql);
        }

        [Fact]
        public void TakeWorksSql() {
            var query = this.GetSelectQuery<Post>().Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select top (@take) [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts]", sql.Sql);
        }

        [Fact]
        public void TakeWorksSql2012WithoutOrder() {
            var query = this.GetSelectQuery<Post>().Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select top (@take) [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts]", sql.Sql);
        }

        [Fact]
        public void TakeWorksSql2012WithOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Title).Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Title] asc offset 0 fetch next @take rows", sql.Sql);
        }

        [Fact]
        public void TakeWorksMySql() {
            var query = this.GetSelectQuery<Post>().Take(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetMySqlWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select `PostId`, `Title`, `Content`, `Rating`, `AuthorId`, `BlogId`, `DoNotMap` from `Posts` limit @take", sql.Sql);
        }

        [Fact]
        public void SkipWorksSql() {
            var query = this.GetSelectQuery<Post>().Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select * from (select ROW_NUMBER() OVER ( order by [PostId]) as RowNum, [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId]) as pagetable where pagetable.RowNum between @skip + 1 and 18446744073709551615 order by pagetable.RowNum", sql.Sql);
        }

        [Fact]
        public void SkipWorksSql2012WithoutOrder() {
            var query = this.GetSelectQuery<Post>().Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId] offset @skip", sql.Sql);
        }

        [Fact]
        public void SkipWorksSql2012WithOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Title).Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Title] asc offset @skip", sql.Sql);
        }

        [Fact]
        public void SkipWorksMySql() {
            var query = this.GetSelectQuery<Post>().Skip(1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetMySqlWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select `PostId`, `Title`, `Content`, `Rating`, `AuthorId`, `BlogId`, `DoNotMap` from `Posts` order by `PostId` limit @skip, 18446744073709551615", sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksSql() {
            var query = this.GetSelectQuery<Post>().Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select * from (select ROW_NUMBER() OVER ( order by [PostId]) as RowNum, [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId]) as pagetable where pagetable.RowNum between @skip + 1 and @skip + @take order by pagetable.RowNum", sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksSql2012WithoutOrder() {
            var query = this.GetSelectQuery<Post>().Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [PostId] offset @skip fetch next @take rows", sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksSql2012WithOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Title).Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetSql2012Writer().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Title] asc offset @skip fetch next @take rows", sql.Sql);
        }

        [Fact]
        public void SkipAndTakeWorksMySql() {
            var query = this.GetSelectQuery<Post>().Skip(1).Take(10);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetMySqlWriter().GenerateSql(selectQuery);
            Debug.Write(sql.Sql);
            Assert.Equal("select `PostId`, `Title`, `Content`, `Rating`, `AuthorId`, `BlogId`, `DoNotMap` from `Posts` order by `PostId` limit @skip, @take", sql.Sql);
        }

        private SelectWriter GetWriter(bool withIgnore = false) {
            return new SelectWriter(new SqlServerDialect(), MakeConfig(withIgnore));
        }

        private SelectWriter GetMySqlWriter(bool withIgnore = false) {
            return new SelectWriter(new MySqlDialect(), MakeConfig(withIgnore));
        }

        private SelectWriter GetSql2012Writer(bool withIgnore = false) {
            return new SelectWriter(new SqlServer2012Dialect(), MakeConfig(withIgnore));
        }

        private SelectQuery<T> GetSelectQuery<T>() {
            var engine = new Mock<IEngine>().Object;
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            return new SelectQuery<T>(engine, connection.Object);
        }

        private static IConfiguration MakeConfig(bool withIgnore = false) {
            if (withIgnore) {
                return new CustomConfigWithIgnore();
            }

            return new CustomConfig();
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new Mock<IEngine>().Object, string.Empty)
            {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigWithIgnore : DefaultConfiguration {
            public CustomConfigWithIgnore()
                : base(new Mock<IEngine>().Object, string.Empty)
            {
                this.AddNamespaceOf<Post>();
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}