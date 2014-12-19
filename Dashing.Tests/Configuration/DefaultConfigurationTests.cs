namespace Dashing.Tests.Configuration {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Engine;

    using Moq;

    using Xunit;

    public class DefaultConfigurationTests {
        private readonly ConnectionStringSettings connectionString = new ConnectionStringSettings {
            ConnectionString = "Data Source=dummy.local",
            ProviderName = "System.Data.SqlClient"
        };

        [Fact]
        public void Constructs() {
            var engine = new Mock<IEngine>();
            Assert.NotNull(new DefaultConfiguration(this.connectionString));
        }
    }
}