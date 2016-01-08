namespace PerformanceTest.Tests.Dashing {
    using System.Configuration;

    using global::Dashing.Configuration;

    using PerformanceTest.Domain;

    internal class DashingConfiguration : DefaultConfiguration {
        public DashingConfiguration()
            : base(new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=tempdb;Integrated Security=True", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Post>();
        }
    }
}