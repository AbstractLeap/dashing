namespace Dashing {
    using System;
    using System.Data;
    using System.Data.Common;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public class SqlDatabase : IDatabase {
        private readonly IConfiguration configuration;

        private readonly DbProviderFactory dbProviderFactory;

        private readonly string connectionString;

        private IEngine engine;

        public bool CompleteFailsSilentlyIfRejected { get; set; }

        public SqlDatabase(IConfiguration configuration, DbProviderFactory dbProviderFactory, string connectionString = null, ISqlDialect sqlDialect = null) {
            this.configuration = configuration;
            this.dbProviderFactory = dbProviderFactory;
            this.connectionString = connectionString;
            this.CompleteFailsSilentlyIfRejected = true;
            this.engine = new SqlEngine(configuration, sqlDialect);
        }

#if !COREFX
        public SqlDatabase(IConfiguration configuration, string connectionString, string providerName = "System.Data.SqlClient")
        : this(configuration, GetDbProviderFactory(providerName), connectionString, GetDialect(providerName, connectionString)) {

        }

        private static ISqlDialect GetDialect(string providerName, string connectionString) {
            var dialectFactory = new DialectFactory();
            return dialectFactory.Create(providerName, connectionString);
        }

        private static DbProviderFactory GetDbProviderFactory(string providerName) {
            return DbProviderFactories.GetFactory(providerName);
        }
#endif

        public ISession BeginSession(IDbConnection connection = null, IDbTransaction transaction = null) {
            return new Session(this.engine, 
                new Lazy<IDbConnection>(() => connection == null ? this.CreateConnection() : connection),
                transaction,
                connection != null,
                transaction == null,
                false,
                this.CompleteFailsSilentlyIfRejected);
        }

        public ISession BeginTransactionLessSession(IDbConnection connection = null) {
            return new Session(this.engine,
                new Lazy<IDbConnection>(() => connection == null ? this.CreateConnection() : connection),
                null,
                connection == null,
                false,
                true,
                this.CompleteFailsSilentlyIfRejected);
        }

        private IDbConnection CreateConnection() {
            var connection = this.dbProviderFactory.CreateConnection();
            connection.ConnectionString = this.connectionString;
            return connection;
        }
    }
}