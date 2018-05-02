namespace Dashing.IntegrationTests.TestDomain.Versioned.NonVersioned {
    using Dashing.Configuration;

    public class NonVersionedConfiguration : BaseConfiguration {
        public NonVersionedConfiguration() {
            this.Add<VersionedEntity>();
        }
    }
}