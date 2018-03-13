namespace Dashing.IntegrationTests.Setup {
    using System.Data.SqlClient;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

#if SQLSERVER
    public class SqlServerDatabase : SqlDatabase {
#else
    class SqlServerDatabase : SqlSessionCreator {
#endif
        public SqlServerDatabase(IConfiguration configuration)
            : base(configuration, SqlClientFactory.Instance, $"Server=localhost;Trusted_Connection=True;MultipleActiveResultSets=True", new SqlServer2012Dialect()) { }
    }
}