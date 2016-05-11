namespace Dashing.IntegrationTests.Setup {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.IntegrationTests.TestDomain.More;

    internal class SqlServerConfiguration : DefaultConfiguration {
        public SqlServerConfiguration()
            : base(new ConnectionStringSettings("Default", "Data Source=(localdb)\\v11.0;Integrated Security=true;MultipleActiveResultSets=True", "System.Data.SqlClient")) {
            this.AddNamespaceOf<Post>();
            this.AddNamespaceOf<Questionnaire>();
        }
    }
}