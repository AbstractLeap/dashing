namespace Dashing.PerformanceTests.Tests.Dashing {
    using global::Dashing.Configuration;
    using global::Dashing.PerformanceTests.Domain;

    internal class DashingConfiguration : BaseConfiguration {
        public DashingConfiguration() {
            this.AddNamespaceOf<Post>();
        }
    }
}