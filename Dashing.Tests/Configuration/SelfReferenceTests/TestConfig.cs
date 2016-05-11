namespace Dashing.Tests.Configuration.SelfReferenceTests {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Tests.Configuration.SelfReferenceTests.Domain;

    public class TestConfig : DefaultConfiguration {
        public TestConfig()
            : base(
                new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=dashingtest;Integrated Security=True", "System.Data.SqlClient")
                ) {
            this.AddNamespaceOf<Post>();
        }
    }
}