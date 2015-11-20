namespace Dashing.IntegrationTests.SqlServer {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.IntegrationTests.TestDomain;

    internal class SqlServerConfiguration : DefaultConfiguration {
        public SqlServerConfiguration()
            : base(new ConnectionStringSettings("Default", "Data Source=(localdb)\\v11.0;Integrated Security=true", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Post>();
        }
    }
}