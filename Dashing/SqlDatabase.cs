namespace Dashing {
    using System;
    using System.Data;
    using System.Data.Common;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;

    public class SqlDatabase : IDatabase {
        protected readonly IConfiguration configuration;

        protected readonly DbProviderFactory dbProviderFactory;

        protected readonly string connectionString;

        protected IEngine engine;

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

        protected static ISqlDialect GetDialect(string providerName, string connectionString) {
            var dialectFactory = new DialectFactory();
            return dialectFactory.Create(providerName, connectionString);
        }

        protected static DbProviderFactory GetDbProviderFactory(string providerName) {
            return DbProviderFactories.GetFactory(providerName);
        }
#endif

        public virtual ISession BeginSession(IDbConnection connection = null, IDbTransaction transaction = null) {
            return new Session(this.engine, 
                new Lazy<IDbConnection>(() => connection ?? this.CreateConnection()),
                transaction,
                connection == null,
                transaction == null,
                false,
                this.CompleteFailsSilentlyIfRejected);
        }

        public virtual ISession BeginTransactionLessSession(IDbConnection connection = null) {
            return new Session(this.engine,
                new Lazy<IDbConnection>(() => connection ?? this.CreateConnection()),
                null,
                connection == null,
                false,
                true,
                this.CompleteFailsSilentlyIfRejected);
        }

        protected IDbConnection CreateConnection() {
            var connection = this.dbProviderFactory.CreateConnection();
            connection.ConnectionString = this.connectionString;
            return connection;
        }
    }
}