namespace Dashing {
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Extensions;

    public partial class Session : ISession, IProjectedSelectQueryExecutor {
        private static readonly Type TypeIEnumerable = typeof(IEnumerable);

        public IDapper Dapper { get; private set; }

        private readonly IEngine engine;

        private readonly Lazy<IDbConnection> connection;

        private IDbTransaction transaction;

        private bool shouldDisposeConnection;

        private bool shouldCommitAndDisposeTransaction;

        private bool isTransactionLess;

        private readonly bool completeFailsSilentlyIfRejected;

        private bool isComplete;

        private bool isRejected;

        private bool isDisposed;

        private readonly object connectionOpenLock = new object();

        private readonly AsyncLock asyncConnectionOpenLock = new AsyncLock();

        #region MethodCaches
        private static readonly ConcurrentDictionary<Type, Func<Session, IEnumerable, int>> InsertMethodsOfType = new ConcurrentDictionary<Type, Func<Session, IEnumerable, int>>();
        private static readonly ConcurrentDictionary<Type, Func<Session, IEnumerable, int>> SaveMethodsOfType = new ConcurrentDictionary<Type, Func<Session, IEnumerable, int>>();
        private static readonly ConcurrentDictionary<Type, Func<Session, IEnumerable, int>> DeleteMethodsOfType = new ConcurrentDictionary<Type, Func<Session, IEnumerable, int>>();

        private static readonly ConcurrentDictionary<Type, Func<Session, IEnumerable, Task<int>>> InsertAsyncMethodsOfType = new ConcurrentDictionary<Type, Func<Session, IEnumerable, Task<int>>>();
        private static readonly ConcurrentDictionary<Type, Func<Session, IEnumerable, Task<int>>> SaveAsyncMethodsOfType = new ConcurrentDictionary<Type, Func<Session, IEnumerable, Task<int>>>();
        private static readonly ConcurrentDictionary<Type, Func<Session, IEnumerable, Task<int>>> DeleteAsyncMethodsOfType = new ConcurrentDictionary<Type, Func<Session, IEnumerable, Task<int>>>();
        #endregion

        public Session(IEngine engine, 
            Lazy<IDbConnection> connection,
            IDbTransaction transaction = null,
            bool disposeConnection = true,
            bool commitAndDisposeTransaction = false,
            bool isTransactionLess = false,
            bool completeFailsSilentlyIfRejected = true) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (transaction != null && isTransactionLess) {
                throw new InvalidOperationException("Unable to start a transaction-less session as transaction is not null");
            }

            if (isTransactionLess && commitAndDisposeTransaction) {
                throw new InvalidOperationException(
                    "As this session is transaction-less it will not be possible to commit and dispose of the transaction");
            }

            this.engine = engine;
            this.connection = connection;
            this.transaction = transaction;
            this.shouldDisposeConnection = disposeConnection;
            this.shouldCommitAndDisposeTransaction = commitAndDisposeTransaction;
            this.isTransactionLess = isTransactionLess;
            this.completeFailsSilentlyIfRejected = completeFailsSilentlyIfRejected;
            this.Dapper = engine.CreateDapperWrapper(this);
        }

        public IConfiguration Configuration
        {
            get
            {
                return this.engine.Configuration;
            }
        }

        internal IDbConnection MaybeOpenConnection() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.connection.Value.State == ConnectionState.Closed || this.connection.Value.State == ConnectionState.Connecting) {
                lock (this.connectionOpenLock) {
                    if (this.connection.Value.State == ConnectionState.Closed) {
                        this.connection.Value.Open();
                    }
                }
            }

            if (this.connection.Value.State != ConnectionState.Open) {
                throw new Exception("Connection in unknown state");
            }

            return this.connection.Value;
        }

        internal IDbTransaction GetTransaction() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.isComplete) {
                throw new InvalidOperationException("Transaction was marked as completed, no further operations are permitted");
            }

            if (this.transaction == null) {
                if (!this.isTransactionLess) {
                    this.transaction = this.MaybeOpenConnection().BeginTransaction();
                    this.shouldCommitAndDisposeTransaction = true;
                }
            }

            return this.transaction;
        }

        internal async Task<IDbConnection> MaybeOpenConnectionAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Session");
            }

            if (this.connection.Value.State == ConnectionState.Closed || this.connection.Value.State == ConnectionState.Connecting) {
                using (await this.asyncConnectionOpenLock.LockAsync()) {
                    if (this.connection.Value.State == ConnectionState.Closed) {
                        await ((DbConnection)this.connection.Value).OpenAsync();
                    }
                }
            }

            if (this.connection.Value.State != ConnectionState.Open) {
                throw new Exception("Connection in unknown state");
            }

            return this.connection.Value;
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
                    var thisConnection = await this.MaybeOpenConnectionAsync();
                    this.transaction = thisConnection.BeginTransaction();
                    this.shouldCommitAndDisposeTransaction = true;
                }
            }

            return this.transaction;
        }

        public virtual void Dispose() {
            if (this.isDisposed) {
                return;
            }

            if (this.transaction != null && this.shouldCommitAndDisposeTransaction) {
                if (!this.isComplete) {
                    this.transaction.Rollback();
                }

                this.transaction.Dispose();
            }

            if (this.shouldDisposeConnection && this.connection.IsValueCreated) {
                this.connection.Value.Dispose();
            }

            this.isDisposed = true;
        }

        public virtual void Complete() {
            if (this.isComplete) {
                throw new InvalidOperationException("Transaction is already complete");
            }

            if (this.isRejected && !this.completeFailsSilentlyIfRejected) {
                throw new InvalidOperationException("This transaction has been rejected");
            }

            if (this.transaction != null && this.shouldCommitAndDisposeTransaction && !this.isRejected) {
                this.transaction.Commit();
            }

            if (!this.isRejected) {
                this.isComplete = true;
            }
        }

        public virtual void Reject() {
            this.isRejected = true;
        }
    }
}