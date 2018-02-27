namespace Dashing.IntegrationTests.Setup {
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    using MySql.Data.MySqlClient;

#if MYSQL
    public class MySqlServerSessionCreator : SqlSessionCreator {
#else
    class MySqlServerSessionCreator : SqlSessionCreator {
#endif
        public MySqlServerSessionCreator(IConfiguration configuration)
            : base(configuration, MySqlClientFactory.Instance, "Server = localhost; Uid=dashingtest;Pwd=SomeDaftPassword;", new MySqlDialect()) { }
    }
}