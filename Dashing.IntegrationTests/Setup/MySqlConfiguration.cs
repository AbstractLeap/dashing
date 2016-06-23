namespace Dashing.IntegrationTests.Setup {
    using System.Configuration;

    using Dashing.Configuration;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.IntegrationTests.TestDomain.More;

    internal class MySqlConfiguration : DefaultConfiguration {
        public MySqlConfiguration()
            : base(new ConnectionStringSettings("Default", "Server=localhost;Uid=dashingtest;Pwd=SomeDaftPassword;", "MySql.Data.MySqlClient")) {
            this.AddNamespaceOf<Post>();
            this.AddNamespaceOf<Questionnaire>();
        }
    }
}