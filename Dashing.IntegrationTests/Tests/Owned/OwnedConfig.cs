namespace Dashing.IntegrationTests.Tests.Owned {
    using Dashing.Configuration;

    public class OwnedConfig : BaseConfiguration {
        public OwnedConfig() {
            this.Add<Owner>();
            this.Add<Owned>();
        }
    }
}