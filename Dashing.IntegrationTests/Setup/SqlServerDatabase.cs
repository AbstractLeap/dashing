namespace Dashing.IntegrationTests.Setup {
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    using Microsoft.Data.SqlClient;

#if SQLSERVER
    public class SqlServerDatabase : SqlDatabase {
#else
    class SqlServerDatabase : SqlSessionCreator {
#endif
        public SqlServerDatabase(IConfiguration configuration)
            : base(configuration, SqlClientFactory.Instance, $"Server=localhost;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=Yes", new SqlServer2012Dialect()) { }
    }
}