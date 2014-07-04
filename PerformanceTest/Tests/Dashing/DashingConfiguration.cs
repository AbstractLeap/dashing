namespace PerformanceTest.Tests.Dashing {
    using System.Configuration;

    using global::Dashing.Configuration;

    using PerformanceTest.Domain;

    internal class DashingConfiguration : DefaultConfiguration {
        public DashingConfiguration(ConnectionStringSettings connectionString)
            : base(connectionString) {
            this.Add<Blog>();
            this.Add<Comment>();
            this.Add<Post>();
            this.Add<User>();
        }
    }
}