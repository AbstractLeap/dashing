namespace TopHat.Tests.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    using Moq;

    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Extensions;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class SelectWriterTests {
        [Fact]
        public void SimpleQueryBuilds() {
            var engine = new SqlServerEngine();
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            var selectWriter = new SelectWriter(new SqlServerDialect(), MakeConfig());
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

        internal class Foo {
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

        private SelectWriter GetWriter(bool withIgnore = false) {
            return new SelectWriter(new SqlServerDialect(), MakeConfig(withIgnore));
        }

        private SelectQuery<T> GetSelectQuery<T>() {
            var engine = new SqlServerEngine();
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
                : base(new SqlServerEngine(), string.Empty) {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigWithIgnore : DefaultConfiguration {
            public CustomConfigWithIgnore()
                : base(new SqlServerEngine(), string.Empty) {
                this.AddNamespaceOf<Post>();
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}