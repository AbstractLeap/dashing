namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;

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

        private bool isTransactionLess;

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
                new Lazy<IDbConnection>(() => this.Connection),
                new Lazy<IDbTransaction>(() => this.Transaction));
        }

        public IConfiguration Configuration {
            get {
                return this.engine.Configuration;
            }
        }

        private IDbConnection Connection {
            get {
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
        }

        private IDbTransaction Transaction {
            get {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("Session");
                }

                if (this.isComplete) {
                    throw new InvalidOperationException(
                        "Transaction was marked as completed, no further operations are permitted");
                }

                if (this.transaction == null) {
                    if (!this.isTransactionLess) {
                        this.transaction = this.Connection.BeginTransaction();
                        this.shouldCommitAndDisposeTransaction = true;
                    }
                }

                return this.transaction;
            }
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
                this.engine.Query<T, TPrimaryKey>(this.Connection, this.Transaction, id);
        }

        public T GetTracked<T, TPrimaryKey>(TPrimaryKey id) {
            return
                this.engine.QueryTracked<T, TPrimaryKey>(this.Connection, this.Transaction, new[] { id })
                    .SingleOrDefault();
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.Query<T, TPrimaryKey>(this.Connection, this.Transaction, ids);
        }

        public IEnumerable<T> GetTracked<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.QueryTracked<T, TPrimaryKey>(this.Connection, this.Transaction, ids);
        }

        public ISelectQuery<T> Query<T>() {
            return new SelectQuery<T>(this);
        }

        public IEnumerable<T> Query<T>(SelectQuery<T> query) {
            return this.engine.Query(this.Connection, this.Transaction, query);
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query) {
            return this.engine.QueryPaged(this.Connection, this.Transaction, query);
        }

        public int Insert<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreInsertListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreInsertListeners) {
                        handler.OnPreInsert(entity, this);
                    }
                }
            }

            var insertedRows = this.engine.Insert(this.Connection, this.Transaction, entities);
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

            var updatedRows = this.engine.Save(this.Connection, this.Transaction, entities);
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
            return this.engine.Execute(this.Connection, this.Transaction, update, predicates);
        }

        public int Delete<T>(IEnumerable<T> entities) {
            if (this.Configuration.EventHandlers.PreDeleteListeners.Any()) {
                foreach (var entity in entities) {
                    foreach (var handler in this.Configuration.EventHandlers.PreDeleteListeners) {
                        handler.OnPreDelete(entity, this);
                    }
                }
            }

            var deletedRows = this.engine.Delete(this.Connection, this.Transaction, entities);
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
            return this.engine.ExecuteBulkDelete(this.Connection, this.Transaction, predicates);
        }

        public int UpdateAll<T>(Action<T> update) {
            return this.engine.Execute(this.Connection, this.Transaction, update, null);
        }

        public int DeleteAll<T>() {
            return this.engine.ExecuteBulkDelete<T>(this.Connection, this.Transaction, null);
        }
    }
}