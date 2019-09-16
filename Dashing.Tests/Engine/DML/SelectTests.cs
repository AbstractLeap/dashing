namespace Dashing.Tests.Engine.DML {
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class SelectTests {
        private readonly ITestOutputHelper outputHelper;

        public SelectTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void SelectAnonymousWorks() {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new {
                                             p.Title
                                         });
            this.AssertSqlMatches("select t.[Title] from [Posts] as t", query);
        }

        [Fact]
        public void SelectBaseWorks() {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new Post {
                                                  Title = p.Title
                                              });
            this.AssertSqlMatches("select t.[Title] from [Posts] as t", query);
        }

        [Fact]
        public void FetchProjectBaseWorks() {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new Post
                                     {
                                         Title = p.Title,
                                         Author = p.Author
                                     });
            this.AssertSqlMatches("select t.[Title], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId", query);
        }

        [Fact]
        public void FetchProjectAnonymousWorks()
        {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new 
                                     {
                                         Title = p.Title,
                                         Author = p.Author
                                     });
            this.AssertSqlMatches("select t.[Title], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId", query);
        }

        private void AssertSqlMatches<TBase, TProjection>(string expected, IProjectedSelectQuery<TBase, TProjection> projectedSelectQuery)
            where TBase : class, new() {
            var selectWriter = this.GetSql2012Writer();
            var concreteQuery = (ProjectedSelectQuery<TBase, TProjection>)projectedSelectQuery;
            var sqlResult = selectWriter.GenerateSql(concreteQuery);
            this.outputHelper.WriteLine(sqlResult.Sql);
            Assert.Equal(expected, sqlResult.Sql);
        }

        private SelectWriter GetSql2012Writer(IConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new CustomConfig();
            }

            return new SelectWriter(new SqlServer2012Dialect(), configuration);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new()
        {
            return new SelectQuery<T>(new Mock<IProjectedSelectQueryExecutor>().Object);
        }

        private class CustomConfig : MockConfiguration
        {
            public CustomConfig()
            {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}