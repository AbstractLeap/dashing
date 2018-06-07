namespace Dashing.IntegrationTests.TestDomain.Versioned {
    using Dashing.Configuration;
    public class VersionedConfiguration : BaseConfiguration {
        public VersionedConfiguration() {
            this.Add<VersionedEntity>();
        }
    }
}