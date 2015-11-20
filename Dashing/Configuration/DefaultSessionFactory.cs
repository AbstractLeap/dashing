namespace Dashing.Configuration {
    using System.Data;

    using Dashing.Engine;

    public class DefaultSessionFactory : ISessionFactory {
        public ISession Create(IEngine engine, IDbConnection connection, IDbTransaction transaction = null, bool disposeConnection = true, bool commitAndDisposeTransaction = false, bool isTransactionLess = false) {
            var sessionState = new SessionState(engine.Configuration, connection, transaction, disposeConnection, commitAndDisposeTransaction, isTransactionLess);
            return new Session(engine, sessionState);
        }
    }
}