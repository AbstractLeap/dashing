namespace Dashing.Configuration {
    using System.Data;

    public interface ISessionFactory {
        ISession Create(IConfiguration config, IDbConnection connection, IDbTransaction transaction = null, bool disposeConnection = true, bool commitAndDisposeTransaction = false);
    }
}