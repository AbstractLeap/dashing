namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    public sealed class Session : ISession {
        private readonly IConfiguration config;

        private readonly IDbConnection connection;

        private readonly bool shouldDisposeConnection;

        private IDbTransaction transaction;

        private bool shouldCommitAndDisposeTransaction;

        private bool isComplete;

        private bool isDisposed;

        public Session(IConfiguration config, IDbConnection connection, IDbTransaction transaction = null, bool disposeConnection = true, bool commitAndDisposeTransaction = false) {
            if (config == null) {
                throw new ArgumentNullException("config");
            }

            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            this.config = config;
            this.connection = connection;
            this.transaction = transaction;
            this.shouldDisposeConnection = disposeConnection;
            this.shouldCommitAndDisposeTransaction = commitAndDisposeTransaction;
        }

        public IDbConnection Connection {
            get {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("Session");
                }

                if (this.connection.State == ConnectionState.Closed) {
                    this.connection.Open();
                }

                if (this.connection.State == ConnectionState.Open) {
                    return this.connection;
                }

                throw new Exception("Connection in unknown state");
            }
        }

        public IDbTransaction Transaction {
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

        public void Complete() {
            if (this.isComplete) {
                throw new InvalidOperationException("Transaction is already complete");
            }

            if (this.transaction != null && this.shouldCommitAndDisposeTransaction) {
                this.transaction.Commit();
            }

            this.isComplete = true;
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

        public T Get<T>(int id, bool? asTracked = null) {
            return this.config.Engine.Get<T>(this.Connection, id, asTracked);
        }

        public T Get<T>(Guid id, bool? asTracked = null) {
            return this.config.Engine.Get<T>(this.Connection, id, asTracked);
        }

        public IEnumerable<T> Get<T>(IEnumerable<int> ids, bool? asTracked = null) {
            return this.config.Engine.Get<T>(this.Connection, ids, asTracked);
        }

        public IEnumerable<T> Get<T>(IEnumerable<Guid> ids, bool? asTracked = null) {
            return this.config.Engine.Get<T>(this.Connection, ids, asTracked);
        }

        public ISelectQuery<T> Query<T>() {
            return new SelectQuery<T>(this.config.Engine, this.Connection);
        }

        public int Insert<T>(params T[] entities) {
            return this.config.Engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
        }

        public int Insert<T>(IEnumerable<T> entities) {
            return this.config.Engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
        }

        public int Update<T>(params T[] entities) {
            return this.config.Engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
        }

        public int Update<T>(IEnumerable<T> entities) {
            return this.config.Engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
        }

        public int Delete<T>(params T[] entities) {
            return this.config.Engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
        }

        public int Delete<T>(IEnumerable<T> entities) {
            return this.config.Engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
        }

        public void UpdateAll<T>(Action<T> update) {
            this.config.Engine.Execute(this.Connection, update, null);
        }

        public void Update<T>(Action<T> update, Expression<Func<T, bool>> predicate) {
            this.config.Engine.Execute(this.Connection, update, new[] { predicate });
        }

        public void Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.config.Engine.Execute(this.Connection, update, predicates);
        }

        public void Update<T>(Action<T> update, params Expression<Func<T, bool>>[] predicates) {
            this.config.Engine.Execute(this.Connection, update, predicates);
        }

        public void DeleteAll<T>() {
            this.config.Engine.ExecuteBulkDelete<T>(this.Connection, null);
        }

        public void Delete<T>(Expression<Func<T, bool>> predicate) {
            this.config.Engine.ExecuteBulkDelete(this.Connection, new[] { predicate });
        }

        public void Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.config.Engine.ExecuteBulkDelete(this.Connection, predicates);
        }

        public void Delete<T>(params Expression<Func<T, bool>>[] predicates) {
            this.config.Engine.ExecuteBulkDelete(this.Connection, predicates);
        }
    }
}