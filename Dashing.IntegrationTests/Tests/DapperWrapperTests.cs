namespace Dashing.IntegrationTests.Tests {
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.Setup;

    using Xunit;

    public class DapperWrapperTests {
        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public async Task DynamicQueryAsyncWorks(TestSessionWrapper wrapper) {
            var result = await wrapper.Session.Dapper.QueryAsync("select 1 as Id");
            Assert.Equal(1, result.Count());
            Assert.Equal(1, result.First().Id);
        }

        [Theory]
        [MemberData("GetSessions", MemberType = typeof(SessionDataGenerator))]
        public async Task StronglyTypedQueryWorks(TestSessionWrapper wrapper) {
            var result = await wrapper.Session.Dapper.QueryAsync<Foo>("select 1 as FooId, 'Rah' as Bar");
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