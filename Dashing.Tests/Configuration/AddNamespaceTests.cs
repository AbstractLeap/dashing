namespace Dashing.Tests.Configuration {
    using System.Linq;

    using Dashing.Tests.Configuration.AddNamespaceDomain;

    using Xunit;

    public class AddNamespaceTests {
        [Fact]
        public void AddNamespaceWorksWithConfigurationInSameNamespace() {
            var config = new AddNamespaceConfiguration();
            Assert.Equal(2, config.Maps.Count());
        }
    }
}