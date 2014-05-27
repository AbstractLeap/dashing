namespace TopHat.Tests.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    using Moq;

    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Tests.TestDomain;

    using Xunit;

    public class SelectWriterTests {
        [Fact]
        public void SimpleQueryBuilds() {
            var engine = new SqlServerEngine();
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            var selectWriter = new SelectWriter(new SqlServerDialect(), MakeMaps());
            var sql = selectWriter.GenerateSql(new SelectQuery<User>(engine, connection.Object));
            Debug.Write(sql);
        }

        [Fact]
        public void FetchTest() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId",
                sql);
        }

        [Fact]
        public void WhereId() {
            var query = this.GetSelectQuery<Post>().Where(p => p.PostId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", sql);
        }

        [Fact]
        public void WhereNonPKFetch() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author).Where(p => p.Author.Username == "bob");
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t_1.[Username] = @l_1)",
                sql);
        }

        [Fact]
        public void WherePKFetch() {
            var query = this.GetSelectQuery<Post>().Fetch(p => p.Author).Where(p => p.Author.UserId == 1);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
            Assert.Equal(
                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t.[AuthorId] = @l_1)",
                sql);
        }

        [Fact]
        public void WhereFetchMultiple() {
            var query = this.GetSelectQuery<Comment>().Fetch(c => c.Post.Author).Where(c => c.Post.Author.Username == "bob");
            var selectQuery = query as SelectQuery<Comment>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
        }

        [Fact]
        public void SimpleOrder() {
            var query = this.GetSelectQuery<Post>().OrderBy(p => p.Rating);
            var selectQuery = query as SelectQuery<Post>;
            var sql = this.GetWriter().GenerateSql(selectQuery);
            Debug.Write(sql);
            Assert.Equal(
                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] order by [Rating] asc",
                sql);
        }

        private SelectWriter GetWriter() {
            return new SelectWriter(new SqlServerDialect(), MakeMaps());
        }

        private SelectQuery<T> GetSelectQuery<T>() {
            var engine = new SqlServerEngine();
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            return new SelectQuery<T>(engine, connection.Object);
        }

        private static IDictionary<Type, IMap> MakeMaps() {
            var config = new CustomConfig();
            return config.Maps.ToDictionary(m => m.Type, m => m);
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new SqlServerEngine(), string.Empty) {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}