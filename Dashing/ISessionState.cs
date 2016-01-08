namespace Dashing {
    using System;
    using System.Data;
    using System.Threading.Tasks;

    public interface ISessionState : IDisposable {
        IDbConnection GetConnection();

        IDbTransaction GetTransaction();

        Task<IDbConnection> GetConnectionAsync();

        Task<IDbTransaction> GetTransactionAsync();

        void Complete();

        void Reject();
    }
}