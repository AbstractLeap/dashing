namespace Dashing.Configuration {
    using System.Data;

    using Dashing.Engine;

    public interface ISessionFactory {
        ISession Create(
            IEngine engine,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool disposeConnection = true,
            bool commitAndDisposeTransaction = false,
            bool isTransactionLess = false);
    }
}