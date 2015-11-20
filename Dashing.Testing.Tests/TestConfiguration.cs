namespace Dashing.Testing.Tests {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Testing.Tests.TestDomain;

    public class TestConfiguration : DefaultConfiguration {
        public TestConfiguration()
            : base(
                new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=dashingtest;Integrated Security=True", "System.Data.SqlClient")
                ) {
            this.AddNamespaceOf<Post>();
        }

        public TestConfiguration(bool useInMemoryEngine)
            : base(
                new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=dashingtest;Integrated Security=True", "System.Data.SqlClient"),
                new InMemoryEngine()) {
            this.AddNamespaceOf<Post>();
        }
    }
}