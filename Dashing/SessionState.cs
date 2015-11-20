namespace Dashing {
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Extensions;

    public class SessionState : ISessionState, IDisposable {
        private readonly IConfiguration configuration;

        private IDbConnection connection;

        private IDbTransaction transaction;

        private bool shouldDisposeConnection;

        private bool shouldCommitAndDisposeTransaction;

        private bool isTransactionLess;

        private bool isComplete;

        private bool isRejected;

        private bool isDisposed;

        private readonly object connectionOpenLock = new object();

        private readonly AsyncLock asyncConnectionOpenLock = new AsyncLock();

        public SessionState(
            IConfiguration configuration,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool disposeConnection = true,
            bool commitAndDisposeTransaction = false,
            bool isTransactionLess = false) {
            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            if (transaction != null && isTransactionLess) {
                throw new InvalidOperationException("Unable to start a transaction-less session as transaction is not null");
            }

            if (isTransactionLess && commitAndDisposeTransaction) {
                throw new InvalidOperationException(
                    "As this session is transaction-less it will not be possible to commit and dispose of the transaction");
            }

            this.configuration = configuration;
            this.connection = connection;
            this.transaction = transaction;
            this.shouldDisposeConnection = disposeConnection;
            this.shouldCommitAndDisposeTransaction = commitAndDisposeTransaction;
            this.isTransactionLess = isTransactionLess;
        }

        public IDbConnection GetConnection() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.connection.State == ConnectionState.Closed || this.connection.State == ConnectionState.Connecting) {
                lock (this.connectionOpenLock) {
                    if (this.connection.State == ConnectionState.Closed) {
                        this.connection.Open();
                    }
                }
            }

            if (this.connection.State != ConnectionState.Open) {
                throw new Exception("Connection in unknown state");
            }

            return this.connection;
        }

        public IDbTransaction GetTransaction() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.isComplete) {
                throw new InvalidOperationException("Transaction was marked as completed, no further operations are permitted");
            }

            if (this.transaction == null) {
                if (!this.isTransactionLess) {
                    this.transaction = this.GetConnection().BeginTransaction();
                    this.shouldCommitAndDisposeTransaction = true;
                }
            }

            return this.transaction;
        }

        public async Task<IDbConnection> GetConnectionAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.connection.State == ConnectionState.Closed || this.connection.State == ConnectionState.Connecting) {
                using (await this.asyncConnectionOpenLock.LockAsync()) {
                    if (this.connection.State == ConnectionState.Closed) {
                        await ((DbConnection)this.connection).OpenAsync();
                    }
                }
            }

            if (this.connection.State != ConnectionState.Open) {
                throw new Exception("Connection in unknown state");
            }

            return this.connection;
        }

        public async Task<IDbTransaction> GetTransactionAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.isComplete) {
                throw new InvalidOperationException("Transaction was marked as completed, no further operations are permitted");
            }

            if (this.transaction == null) {
                if (!this.isTransactionLess) {
                    var thisConnection = await this.GetConnectionAsync();
                    this.transaction = thisConnection.BeginTransaction();
                    this.shouldCommitAndDisposeTransaction = true;
                }
            }

            return this.transaction;
        }

        public void Complete() {
            if (this.isComplete) {
                throw new InvalidOperationException("Transaction is already complete");
            }

            if (this.isRejected && !this.configuration.CompleteFailsSilentlyIfRejected) {
                throw new InvalidOperationException("This transaction has been rejected");
            }

            if (this.transaction != null && this.shouldCommitAndDisposeTransaction && !this.isRejected) {
                this.transaction.Commit();
            }

            if (!this.isRejected) {
                this.isComplete = true;
            }
        }

        public void Reject() {
            this.isRejected = true;
        }

        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            if (this.transaction != null && this.shouldCommitAndDisposeTransaction) {
                if (!this.isComplete) {
                    this.transaction.Rollback();
                }

                this.transaction.Dispose();
            }

            if (this.shouldDisposeConnection) {
                this.connection.Dispose();
            }

            this.isDisposed = true;
        }
    }
}