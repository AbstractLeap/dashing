namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Engine;

    public sealed class Session : ISession {
        public IDapper Dapper { get; private set; }

        private readonly IEngine engine;

        private readonly IDbConnection connection;

        private readonly bool shouldDisposeConnection;

        private IDbTransaction transaction;

        private bool shouldCommitAndDisposeTransaction;

        private bool isComplete;

        private bool isDisposed;

        public Session(IEngine engine, IDbConnection connection, IDbTransaction transaction = null, bool disposeConnection = true, bool commitAndDisposeTransaction = false) {
            if (engine == null) {
                throw new ArgumentNullException("engine");
            }

            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            this.engine = engine;
            this.connection = connection;
            this.transaction = transaction;
            this.shouldDisposeConnection = disposeConnection;
            this.shouldCommitAndDisposeTransaction = commitAndDisposeTransaction;
            this.Dapper = new DapperWrapper(new Lazy<IDbConnection>(() => this.Connection), new Lazy<IDbTransaction>(() => this.Transaction));
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
                    throw new InvalidOperationException("Transaction was marked as completed, no further operations are permitted");
                }

                if (this.transaction == null) {
                    this.transaction = this.Connection.BeginTransaction();
                    this.shouldCommitAndDisposeTransaction = true;
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
            return this.engine.Query<T, TPrimaryKey>(this.Transaction, new[] { id }).SingleOrDefault();
        }

        public T GetTracked<T, TPrimaryKey>(TPrimaryKey id) {
            return this.engine.QueryTracked<T, TPrimaryKey>(this.Transaction, new[] { id }).SingleOrDefault();
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.Query<T, TPrimaryKey>(this.Transaction, ids);
        }

        public IEnumerable<T> GetTracked<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.engine.QueryTracked<T, TPrimaryKey>(this.Transaction, ids);
        }

        public ISelectQuery<T> Query<T>() {
            return new SelectQuery<T>(this.engine, this.Transaction);
        }

        public int Insert<T>(IEnumerable<T> entities) {
            return this.engine.Insert(this.Transaction, entities);
        }

        public int Save<T>(IEnumerable<T> entities) {
            return this.engine.Save(this.Transaction, entities);
        }

        public int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            return this.engine.Execute(this.Transaction, update, predicates);
        }

        public int Delete<T>(IEnumerable<T> entities) {
            return this.engine.Delete(this.Transaction, entities);
        }

        public int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            return this.engine.ExecuteBulkDelete(this.Transaction, predicates);
        }

        public int UpdateAll<T>(Action<T> update) {
            return this.engine.Execute(this.Transaction, update, null);
        }

        public int DeleteAll<T>() {
            return this.engine.ExecuteBulkDelete<T>(this.Transaction, null);
        }
    }
}