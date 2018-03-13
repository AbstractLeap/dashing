namespace Dashing.IntegrationTests.Setup {
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    using MySql.Data.MySqlClient;

#if MYSQL
    public class MySqlServerDatabase : SqlSessionCreator {
#else
    class MySqlServerDatabase : SqlDatabase {
#endif
        public MySqlServerDatabase(IConfiguration configuration)
            : base(configuration, MySqlClientFactory.Instance, "Server = localhost; Uid=dashingtest;Pwd=SomeDaftPassword;", new MySqlDialect()) { }
    }
}