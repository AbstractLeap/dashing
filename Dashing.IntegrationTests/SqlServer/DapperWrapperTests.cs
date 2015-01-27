namespace Dashing.IntegrationTests.SqlServer {
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.SqlServer.Fixtures;

    using Xunit;

    public class DapperWrapperTests : IClassFixture<SqlServerFixture> {
        private readonly SqlServerFixture fixture;

        public DapperWrapperTests(SqlServerFixture fixture) {
            this.fixture = fixture;
        }

        [Fact]
        public async Task DynamicQueryAsyncWorks() {
            var result = await this.fixture.Session.Dapper.QueryAsync("select 1 as Id");
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task StronglyTypedQueryWorks() {
            var result = await this.fixture.Session.Dapper.QueryAsync<Foo>("select 1 as FooId, 'Rah' as Bar");
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result.First().FooId);
            Assert.Equal("Rah", result.First().Bar);
        }

        public class Foo {
            public int FooId { get; set; }

            public string Bar { get; set; }
        }
    }
}