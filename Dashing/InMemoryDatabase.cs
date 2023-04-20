namespace Dashing {
    using System.Data;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.InMemory;

    public class InMemoryDatabase : IDatabase {
        private readonly IConfiguration configuration;

        private InMemoryEngine engine;

        public bool CompleteFailsSilentlyIfRejected { get; set; }

        public InMemoryDatabase(IConfiguration configuration) {
            this.configuration = configuration;
            this.engine = new InMemoryEngine(configuration);
        }

        public virtual ISession BeginSession(IDbConnection connection = null, IDbTransaction transaction = null) {
            return new Session(this.engine,
                new System.Lazy<IDbConnection>(() => new InMemoryDbConnection()),
                null,
                false,
                false,
                true,
                this.CompleteFailsSilentlyIfRejected);
        }

        public virtual ISession BeginTransactionLessSession(IDbConnection connection = null) {
            return new Session(this.engine,
                new System.Lazy<IDbConnection>(() => new InMemoryDbConnection()),
                null,
                false,
                false,
                true,
                this.CompleteFailsSilentlyIfRejected);
        }
    }
}