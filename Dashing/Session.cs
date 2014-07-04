namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dashing.Configuration;
    using System.Linq.Expressions;

    /// <summary>
    ///     The session.
    /// </summary>
    public sealed class Session : ISession {
        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly IDbConnection connection;

        /// <summary>
        ///     The code manager
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        ///     The _is their transaction.
        /// </summary>
        private readonly bool isTheirTransaction;

        /// <summary>
        ///     The _transaction.
        /// </summary>
        private IDbTransaction transaction;

        /// <summary>
        ///     The _is disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        ///     The _is completed.
        /// </summary>
        private bool isCompleted;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public Session(IDbConnection connection, IConfiguration config)
            : this(connection, config, null) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Session" /> class.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <param name="transaction">
        ///     The transaction.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public Session(IDbConnection connection, IConfiguration config, IDbTransaction transaction = null) {
            if (connection == null) {
                throw new ArgumentNullException("connection");
            }

            if (config == null) {
                throw new ArgumentNullException("config");
            }

            this.config = config;
            this.connection = connection;

            if (transaction != null) {
                this.isTheirTransaction = true;
                this.transaction = transaction;
            }
        }

        /// <summary>
        ///     Gets the connection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
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

        /// <summary>
        ///     Gets the transaction.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        /// </exception>
        public IDbTransaction Transaction {
            get {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("Session");
                }

                return this.transaction ?? (this.transaction = this.Connection.BeginTransaction());
            }
        }

        /// <summary>
        ///     The complete.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void Complete() {
            if (this.isCompleted) {
                throw new InvalidOperationException("Only call complete once, when all of the transactional work is done");
            }

            if (this.transaction != null && !this.isTheirTransaction) {
                this.transaction.Commit();
            }

            this.isCompleted = true;
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            if (this.transaction != null && !this.isTheirTransaction) {
                if (!this.isCompleted) {
                    this.transaction.Rollback();
                }

                this.transaction.Dispose();
            }

            this.isDisposed = true;
        }

        public T Get<T>(int id, bool? asTracked = null)
        {
            return this.config.Engine.Get<T>(this.connection, id, asTracked);
        }

        public T Get<T>(Guid id, bool? asTracked = null)
        {
            return this.config.Engine.Get<T>(this.connection, id, asTracked);
        }

        public IEnumerable<T> Get<T>(IEnumerable<int> ids, bool? asTracked = null)
        {
            return this.config.Engine.Get<T>(this.connection, ids, asTracked);
        }

        public IEnumerable<T> Get<T>(IEnumerable<Guid> ids, bool? asTracked = null)
        {
            return this.config.Engine.Get<T>(this.connection, ids, asTracked);
        }

        /// <summary>
        ///     The query.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Query<T>() {
            return new SelectQuery<T>(this.config.Engine, this.Connection);
        }

        /// <summary>
        ///     The insert.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Insert<T>(params T[] entities) {
            return this.config.Engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
        }

        /// <summary>
        ///     The insert.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Insert<T>(IEnumerable<T> entities) {
            return this.config.Engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
        }

        /// <summary>
        ///     The update.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Update<T>(params T[] entities) {
            return this.config.Engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
        }

        /// <summary>
        ///     The update.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Update<T>(IEnumerable<T> entities) {
            return this.config.Engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
        }

        /// <summary>
        ///     The delete.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public int Delete<T>(params T[] entities) {
            return this.config.Engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
        }

        /// <summary>
        ///     The delete.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
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