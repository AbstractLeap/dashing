namespace Dashing.IntegrationTests.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.Setup;
    using Dashing.IntegrationTests.TestDomain.PrivateFields;

    using Xunit;
    using Xunit.Abstractions;

    public class DapperWrapperTests {
        private readonly ITestOutputHelper output;

        public DapperWrapperTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task DynamicQueryAsyncWorks(TestSessionWrapper wrapper) {
            var result = await wrapper.Session.Dapper.QueryAsync("select 1 as Id");
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Theory]
        [MemberData(nameof(SessionDataGenerator.GetSessions), MemberType = typeof(SessionDataGenerator))]
        public async Task StronglyTypedQueryWorks(TestSessionWrapper wrapper) {
            var result = await wrapper.Session.Dapper.QueryAsync<Foo>("select 1 as FooId, 'Rah' as Bar");
            Assert.Single(result);
            Assert.Equal(1, result.First().FooId);
            Assert.Equal("Rah", result.First().Bar);
        }

        [Fact]
        public async Task PrivateFieldsWorkInDapper() {
            var config = new Configuration();
            var database = new SqlServerDatabase(config);
            using (var session = database.BeginSession()) {
                var id = Guid.NewGuid();
                var privateEntity = await session.Dapper.QueryAsync<PrivateEntity>($"select cast('{id}' as uniqueidentifier) as Id, 'Foo' as name");
                Assert.Equal(
                    $"{id}:Foo",
                    privateEntity.First()
                                 .ToString());
            }
        }

        public class Foo {
            public int FooId { get; set; }

            public string Bar { get; set; }
        }
    }
}