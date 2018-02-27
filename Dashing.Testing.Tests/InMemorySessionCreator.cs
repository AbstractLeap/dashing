namespace Dashing.Testing.Tests {
    using System.Data;

    using Dashing.Configuration;

    public class InMemorySessionCreator : ISessionCreator {
        private readonly IConfiguration configuration;

        private InMemoryEngine engine;

        public bool CompleteFailsSilentlyIfRejected { get; set; }

        public InMemorySessionCreator(IConfiguration configuration) {
            this.configuration = configuration;
            this.engine = new InMemoryEngine(configuration);
        }

        public ISession BeginSession(IDbConnection connection = null, IDbTransaction transaction = null) {
            return new Session(this.engine,
                new System.Lazy<IDbConnection>(() => new InMemoryDbConnection()),
                null,
                false,
                false,
                true,
                this.CompleteFailsSilentlyIfRejected);
        }

        public ISession BeginTransactionLessSession(IDbConnection connection = null) {
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