namespace Dashing.Configuration {
    using System.Data;

    public class DefaultSessionFactory : ISessionFactory {
        public ISession Create(IConfiguration config, IDbConnection connection, IDbTransaction transaction = null, bool disposeConnection = true, bool commitAndDisposeTransaction = false) {
            return new Session(config, connection, transaction, disposeConnection, commitAndDisposeTransaction);
        }
    }
}