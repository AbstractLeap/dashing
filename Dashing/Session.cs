namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Extensions;

    public sealed class Session : ISession, ISelectQueryExecutor {
        public IDapper Dapper { get; private set; }

        private readonly IEngine engine;

        private readonly IDbConnection connection;

        private readonly bool shouldDisposeConnection;

        private IDbTransaction transaction;

        private bool shouldCommitAndDisposeTransaction;

        private bool isComplete;

        private bool isDisposed;

        private bool isRejected;

        private readonly bool isTransactionLess;

        private readonly object connectionOpenLock = new object();

        private readonly AsyncLock asyncConnectionOpenLock = new AsyncLock();

        public Session(
            IEngine engine,
            IDbConnection connection,
            IDbTransaction transaction = null,
            bool disposeConnection = true,
            bool commitAndDisposeTransaction = false,
            bool isTransactionLess = false) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

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

            this.engine = engine;
            this.connection = connection;
            this.transaction = transaction;
            this.shouldDisposeConnection = disposeConnection;
            this.shouldCommitAndDisposeTransaction = commitAndDisposeTransaction;
            this.isTransactionLess = isTransactionLess;
            this.Dapper = new DapperWrapper(
                new Lazy<IDbConnection>(this.GetConnection),
                new Lazy<IDbTransaction>(this.GetTransaction),
                new AsyncLazy<IDbConnection>(() => this.GetConnectionAsync()),
                new AsyncLazy<IDbTransaction>(() => this.GetTransactionAsync()));
        }

        public IConfiguration Configuration {
            get {
                return this.engine.Configuration;
            }
        }

        private IDbConnection GetConnection() {
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

        private async Task<IDbConnection> GetConnectionAsync() {
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

        private IDbTransaction GetTransaction() {
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

        private async Task<IDbTransaction> GetTransactionAsync() {
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

        public void Complete() {
            if (this.isComplete) {
                throw new InvalidOperationException("Transaction is already complete");
            }

            if (this.isRejected && !this.Configuration.CompleteFailsSilentlyIfRejected) {
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

        public T Get<T, TPrimaryKey>(TPrimaryKey id) {
            return this.engine.Query<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), id, true);
        }

        public T GetNonTracked<T, TPrimaryKey>(TPrimaryKey id) {
            return this.engine.Query<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), new[] { id }, false).SingleOrDefault();
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.Query<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), ids, true);
        }

        public IEnumerable<T> GetNonTracked<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.Query<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), ids, false);
        }

        public ISelectQuery<T> Query<T>() {
            return new SelectQuery<T>(this);
        }

        public IEnumerable<T> Query<T>(SelectQuery<T> query) {
            return this.engine.Query(this.GetConnection(), this.GetTransaction(), query);
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query) {
            return this.engine.QueryPaged(this.GetConnection(), this.GetTransaction(), query);
        }

        public int Count<T>(SelectQuery<T> query) {
            return this.engine.Count(this.GetConnection(), this.GetTransaction(), query);
        }

        public int Insert<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = this.engine.Insert(this.GetConnection(), this.GetTransaction(), entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public int Save<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = this.engine.Save(this.GetConnection(), this.GetTransaction(), entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            return this.engine.Execute(this.GetConnection(), this.GetTransaction(), update, predicates);
        }

        public int Delete<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = this.engine.Delete(this.GetConnection(), this.GetTransaction(), entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            return this.engine.ExecuteBulkDelete(this.GetConnection(), this.GetTransaction(), predicates);
        }

        public int UpdateAll<T>(Action<T> update) {
            return this.engine.Execute(this.GetConnection(), this.GetTransaction(), update, null);
        }

        public int DeleteAll<T>() {
            return this.engine.ExecuteBulkDelete<T>(this.GetConnection(), this.GetTransaction(), null);
        }

        public async Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id) {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.GetConnectionAsync(), await this.GetTransactionAsync(), id, true);
        }

        public async Task<T> GetNonTrackedAsync<T, TPrimaryKey>(TPrimaryKey id) {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.GetConnectionAsync(), await this.GetTransactionAsync(), id, false);
        }

        public async Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.GetConnectionAsync(), await this.GetTransactionAsync(), ids, true);
        }

        public async Task<IEnumerable<T>> GetNonTrackedAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.GetConnectionAsync(), await this.GetTransactionAsync(), ids, false);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) {
            return await this.engine.QueryAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) {
            return await this.engine.QueryPagedAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<int> CountAsync<T>(SelectQuery<T> query) {
            return await this.engine.CountAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<int> InsertAsync<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = await this.engine.InsertAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public async Task<int> SaveAsync<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = await this.engine.SaveAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public async Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            return await this.engine.ExecuteAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), update, predicates);
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = await this.engine.DeleteAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            return await this.engine.ExecuteBulkDeleteAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), predicates);
        }

        public async Task<int> UpdateAllAsync<T>(Action<T> update) {
            return await this.engine.ExecuteAsync(await this.GetConnectionAsync(), await this.GetTransactionAsync(), update, null);
        }

        public async Task<int> DeleteAllAsync<T>() {
            return await this.engine.ExecuteBulkDeleteAsync<T>(await this.GetConnectionAsync(), await this.GetTransactionAsync(), null);
        }
    }
}