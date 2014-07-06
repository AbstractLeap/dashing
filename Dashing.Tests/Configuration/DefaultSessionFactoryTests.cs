namespace Dashing.Tests.Configuration {
    using System.Data;

    using Dashing.Configuration;
    using Dashing.Engine;

    using Moq;

    using Xunit;

    public class DefaultSessionFactoryTests {
        private readonly Mock<IEngine> mockEngine = new Mock<IEngine>();

        private readonly Mock<IDbConnection> mockConnection = new Mock<IDbConnection>();

        private readonly Mock<IDbTransaction> mockTransaction = new Mock<IDbTransaction>();

        private readonly Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();

        [Fact]
        public void CreateReturnsASession() {
            var target = this.MakeTarget();
            Assert.IsType<Session>(target.Create(this.mockConnection.Object, this.mockConfig.Object));
        }

        [Fact]
        public void CreateWithTransactionReturnsASession() {
            var target = this.MakeTarget();
            Assert.IsType<Session>(target.Create(this.mockConnection.Object, this.mockTransaction.Object, this.mockConfig.Object));
        }

        private DefaultSessionFactory MakeTarget() {
            return new DefaultSessionFactory();
        }
    }
}