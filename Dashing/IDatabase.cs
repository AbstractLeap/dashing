namespace Dashing {
    using System.Data;

    public interface IDatabase {
        ISession BeginSession(IDbConnection connection = null, IDbTransaction transaction = null);

        ISession BeginTransactionLessSession(IDbConnection connection = null);
    }
}