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

    public sealed class Session : ISession, IExecuteSelectQueries {
        public IDapper Dapper { get; private set; }

        private readonly IEngine engine;

        private readonly IDbConnection connection;

        private readonly bool shouldDisposeConnection;

        private IDbTransaction transaction;

        private bool shouldCommitAndDisposeTransaction;

        private bool isComplete;

        private bool isDisposed;

        private readonly bool isTransactionLess;

        public Session(IEngine engine, IDbConnection connection, IDbTransaction transaction = null, bool disposeConnection = true, bool commitAndDisposeTransaction = false, bool isTransactionLess = false) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            if (transaction != null && isTransactionLess) {
                throw new InvalidOperationException(
                    "Unable to start a transaction-less session as transaction is not null");
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
                new Lazy<IDbTransaction>(this.GetTransaction));
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

            if (this.connection.State == ConnectionState.Closed) {
                this.connection.Open();
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

            if (this.connection.State == ConnectionState.Closed) {
                await ((DbConnection)this.connection).OpenAsync();
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
                throw new InvalidOperationException(
                    "Transaction was marked as completed, no further operations are permitted");
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
                throw new InvalidOperationException(
                    "Transaction was marked as completed, no further operations are permitted");
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

            if (this.transaction != null && this.shouldCommitAndDisposeTransaction) {
                this.transaction.Commit();
            }

            this.isComplete = true;
        }

        public T Get<T, TPrimaryKey>(TPrimaryKey id) {
            return
                this.engine.Query<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), id);
        }

        public T GetTracked<T, TPrimaryKey>(TPrimaryKey id) {
            return
                this.engine.QueryTracked<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), new[] { id })
                    .SingleOrDefault();
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.Query<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), ids);
        }

        public IEnumerable<T> GetTracked<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.QueryTracked<T, TPrimaryKey>(this.GetConnection(), this.GetTransaction(), ids);
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
            return await this.engine.QueryAsync<T, TPrimaryKey>(await this.GetConnectionAsync(), await this.GetTransactionAsync(), id);
        }
    }
}