namespace Dashing.Tests.Configuration {
    using Dashing.Configuration;
    using Dashing.Engine;

    using Moq;

    using Xunit;

    public class DefaultConfigurationTests {
        private const string ConnectionString = "Host=dummy.local";

        [Fact]
        public void Constructs() {
            var engine = new Mock<IEngine>();
            Assert.NotNull(new DefaultConfiguration(engine.Object, ConnectionString));
        }
    }
}