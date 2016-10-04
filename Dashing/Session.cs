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
            this.Dapper = new DapperWrapper(
                new Lazy<IDbConnection>(this.MaybeOpenConnection), 
                new Lazy<IDbTransaction>(this.GetTransaction), 
                new AsyncLazy<IDbConnection>(() => this.MaybeOpenConnectionAsync()), 
                new AsyncLazy<IDbTransaction>(() => this.GetTransactionAsync()));
        }

        public IConfiguration Configuration
        {
            get
            {
                return this.engine.Configuration;
            }
        }

        private IDbConnection MaybeOpenConnection() {
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

        private IDbTransaction GetTransaction() {
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

        private async Task<IDbConnection> MaybeOpenConnectionAsync() {
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

            if (this.shouldDisposeConnection && this.connection.IsValueCreated) {
                this.connection.Value.Dispose();
            }

            this.isDisposed = true;
        }

        public void Complete() {
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

        public void Reject() {
            this.isRejected = true;
        }

        public T Get<T, TPrimaryKey>(TPrimaryKey id) where T : class, new() {
            return this.engine.Query<T, TPrimaryKey>(this.MaybeOpenConnection(), this.GetTransaction(), id);
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return this.engine.Query<T, TPrimaryKey>(this.MaybeOpenConnection(), this.GetTransaction(), ids);
        }

        public ISelectQuery<T> Query<T>() where T : class, new() {
            return new SelectQuery<T>(this);
        }

        public IEnumerable<T> Query<T>(SelectQuery<T> query) where T : class, new() {
            return this.engine.Query(this.MaybeOpenConnection(), this.GetTransaction(), query);
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query) where T : class, new() {
            return this.engine.QueryPaged(this.MaybeOpenConnection(), this.GetTransaction(), query);
        }

        public int Count<T>(SelectQuery<T> query) where T : class, new() {
            return this.engine.Count(this.MaybeOpenConnection(), this.GetTransaction(), query);
        }

        public int Insert<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = this.engine.Insert(this.MaybeOpenConnection(), this.GetTransaction(), entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public int Save<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = this.engine.Save(this.MaybeOpenConnection(), this.GetTransaction(), entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            if (predicates == null || !predicates.Any()) {
                throw new ArgumentException("You must provide at least 1 predicate to Update. If you wish to update all entities use UpdateAll");
            }

            return this.engine.Execute(this.MaybeOpenConnection(), this.GetTransaction(), update, predicates);
        }

        public int Delete<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = this.engine.Delete(this.MaybeOpenConnection(), this.GetTransaction(), entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return this.engine.ExecuteBulkDelete(this.MaybeOpenConnection(), this.GetTransaction(), predicates);
        }

        public int UpdateAll<T>(Action<T> update) where T : class, new() {
            return this.engine.Execute(this.MaybeOpenConnection(), this.GetTransaction(), update, null);
        }

        public int DeleteAll<T>() where T : class, new() {
            return this.engine.ExecuteBulkDelete<T>(this.MaybeOpenConnection(), this.GetTransaction(), null);
        }

        public async Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id) where T : class, new() {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), id);
        }

        public async Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), ids);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) where T : class, new() {
            return await this.engine.QueryAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) where T : class, new() {
            return await this.engine.QueryPagedAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<int> CountAsync<T>(SelectQuery<T> query) where T : class, new() {
            return await this.engine.CountAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), query);
        }

        public async Task<int> InsertAsync<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = await this.engine.InsertAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostInsertListeners) {
                        handler.OnPostInsert(entity, this);
                    }
                }
            }

            return insertedRows;
        }

        public async Task<int> SaveAsync<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreSaveListeners) {
                        handler.OnPreSave(entity, this);
                    }
                }
            }

            var updatedRows = await this.engine.SaveAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostSaveListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostSaveListeners) {
                        handler.OnPostSave(entity, this);
                    }
                }
            }

            return updatedRows;
        }

        public async Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            if (predicates == null || !predicates.Any()) {
                throw new ArgumentException("You must provide at least 1 predicate to Update. If you wish to update all entities use UpdateAll");
            }

            return await this.engine.ExecuteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), update, predicates);
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<T> entities) where T : class, new() {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = await this.engine.DeleteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), entities);
            if (this.Configuration.EventHandlers.PostDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PostDeleteListeners) {
                        handler.OnPostDelete(entity, this);
                    }
                }
            }

            return deletedRows;
        }

        public async Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return await this.engine.ExecuteBulkDeleteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), predicates);
        }

        public async Task<int> UpdateAllAsync<T>(Action<T> update) where T : class, new() {
            return await this.engine.ExecuteAsync(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), update, null);
        }

        public async Task<int> DeleteAllAsync<T>() where T : class, new() {
            return await this.engine.ExecuteBulkDeleteAsync<T>(await this.MaybeOpenConnectionAsync(), await this.GetTransactionAsync(), null);
        }
    }
}