namespace Dashing.IntegrationTests.Configuration {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.IntegrationTests.Configuration.Domain;

    public class TestConfig : DefaultConfiguration {
        public TestConfig()
            : base(new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=dashingtest;Integrated Security=True", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Post>();
        }
    }
}