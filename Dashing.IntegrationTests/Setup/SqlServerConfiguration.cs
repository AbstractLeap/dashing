namespace Dashing.IntegrationTests.Setup {
    using System.Data.SqlClient;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

#if SQLSERVER
    public class SqlServerSessionCreator : SqlSessionCreator {
#else
    class SqlServerSessionCreator : SqlSessionCreator {
#endif
        public SqlServerSessionCreator(IConfiguration configuration)
            : base(configuration, SqlClientFactory.Instance, $"Server=localhost;Trusted_Connection=True;MultipleActiveResultSets=True", new SqlServer2012Dialect()) { }
    }
}