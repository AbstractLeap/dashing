namespace Dashing.IntegrationTests.SqlServer {
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class AnyTests : IClassFixture<SqlServerFixture> {
        private readonly SqlServerFixture fixture;

        public AnyTests(SqlServerFixture data) {
            this.fixture = data;
        }

        [Fact]
        public async Task AnyAsyncTrueWorks() {
            Assert.True(await this.fixture.Session.Query<User>().Where(u => u.UserId == 8).AnyAsync());
        }

        [Fact]
        public async Task AnyAsyncFalseWorks() {
            Assert.False(await this.fixture.Session.Query<User>().Where(u => u.UserId == 15).AnyAsync());
        }

        [Fact]
        public async Task AnyAsyncWhereTrueWorks() {
            Assert.True(await this.fixture.Session.Query<User>().AnyAsync(u => u.UserId == 8));
        }

        [Fact]
        public async Task AnyAsyncWhereFalseWorks() {
            Assert.False(await this.fixture.Session.Query<User>().AnyAsync(u => u.UserId == 15));
        }

        [Fact]
        public  void AnyTrueWorks() {
            Assert.True(this.fixture.Session.Query<User>().Where(u => u.UserId == 8).Any());
        }

        [Fact]
        public  void AnyFalseWorks() {
            Assert.False(this.fixture.Session.Query<User>().Where(u => u.UserId == 15).Any());
        }

        [Fact]
        public  void AnyWhereTrueWorks() {
            Assert.True(this.fixture.Session.Query<User>().Any(u => u.UserId == 8));
        }

        [Fact]
        public  void AnyWhereFalseWorks() {
            Assert.False(this.fixture.Session.Query<User>().Any(u => u.UserId == 15));
        }
    }
}