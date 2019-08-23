namespace Dashing.Tests.Engine.DML {
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class SelectTests {
        [Fact]
        public void SelectAnonymousWorks() {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new {
                                             p.Title
                                         });
        }

        [Fact]
        public void SelectBaseWorks() {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new Post {
                                                  Title = p.Title
                                              });
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