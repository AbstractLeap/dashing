namespace PerformanceTest.Tests.Dashing {
    using System.Configuration;

    using global::Dashing.Configuration;

    using PerformanceTest.Domain;

    internal class DashingConfiguration : DefaultConfiguration {
        public DashingConfiguration(ConnectionStringSettings connectionStringSettings)
            : base(connectionStringSettings) {
            this.AddNamespaceOf<Post>();
        }
    }
}