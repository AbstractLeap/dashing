namespace Dashing.Tests.Engine {
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class CountWriterTests {
        [Fact]
        public void SimpleQueryReturnsCorrectSql() {
            var target = MakeTarget();
            var query = MakeUserQuery();

            var sql = target.GenerateCountSql(query);

            Assert.Equal("select count(1) from [Users]", sql.Sql);
        }

        [Fact]
        public void WheredQueryReturnsCorrectSql() {
            var target = MakeTarget();
            var query = MakeUserQuery();

            query.Where(u => u.HeightInMeters < 1);
            var sql = target.GenerateCountSql(query);

            Assert.Equal("select count(1) from [Users] where ([HeightInMeters] < @l_1)", sql.Sql);
        }

        [Fact]
        public void FetchingManyToOneWheredQueryReturnsCorrectSql() {
            var target = MakeTarget();
            var query = MakePostQuery();

            query.Fetch(p => p.Author).Where(p => p.Author.HeightInMeters < 1);
            var sql = target.GenerateCountSql(query);

            Assert.Equal("select count(1) from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where (t_1.[HeightInMeters] < @l_1)", sql.Sql);
        }

        [Fact]
        public void FetchingOneToManyWheredQueryReturnsCorrectSql() {
            var target = MakeTarget();
            var query = MakePostQuery();

            query.Fetch(p => p.Comments);
            var sql = target.GenerateCountSql(query);

            Assert.Equal("select count(distinct [PostId]) from [Posts] as t left join [Comments] as t_1 on t.PostId = t_1.PostId", sql.Sql);
        }

        private static SelectQuery<User> MakeUserQuery() {
            return new SelectQuery<User>(new Mock<IExecuteSelectQueries>().Object);
        }

        private static SelectQuery<Post> MakePostQuery() {
            return new SelectQuery<Post>(new Mock<IExecuteSelectQueries>().Object);
        }

        private static ICountWriter MakeTarget() {
            var dialect = new SqlServerDialect();
            var configuration = MakeConfig();
            var selectWriter = new CountWriter(dialect, configuration);
            return selectWriter;
        }

        private static IConfiguration MakeConfig() {
            return new CustomConfig();
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}