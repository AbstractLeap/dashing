namespace Dashing.Tests {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.Tests.TestDomain;

    public class TestConfig : DefaultConfiguration {
        public TestConfig()
            : base(new ConnectionStringSettings("Default", "", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Post>();
        }
    }
}