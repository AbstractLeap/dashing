namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using TopHat.Configuration;

    /// <summary>
    ///     The engine base.
    /// </summary>
    public abstract class EngineBase : IEngine {
        public IConfiguration Configuration { get; set; }

        protected ISelectWriter SelectWriter { get; set; }

        protected IEntitySqlWriter UpdateWriter { get; set; }

        protected IEntitySqlWriter InsertWriter { get; set; }

        protected IEntitySqlWriter DeleteWriter { get; set; }

        /// <summary>
        ///     Gets or sets the maps.
        /// </summary>
        protected IDictionary<Type, IMap> Maps { get; set; }

        protected ISqlDialect Dialect { get; set; }

        public EngineBase(ISqlDialect dialect) {
            this.Dialect = dialect;
        }

        /// <summary>
        ///     The open.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        /// <returns>
        ///     The <see cref="IDbConnection" />.
        /// </returns>
        public IDbConnection Open(string connectionString) {
            var connection = this.NewConnection(connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        ///     The use maps.
        /// </summary>
        /// <param name="maps">
        ///     The maps.
        /// </param>
        public void UseMaps(IDictionary<Type, IMap> maps) {
            this.Maps = maps;
            this.SelectWriter = new SelectWriter(this.Dialect, this.Configuration);
            this.DeleteWriter = new DeleteWriter(this.Dialect, this.Configuration);
            this.UpdateWriter = new UpdateWriter(this.Dialect, this.Configuration);
            this.InsertWriter = new InsertWriter(this.Dialect, this.Configuration);
        }

        /// <summary>
        ///     The new connection.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        /// <returns>
        ///     The <see cref="IDbConnection" />.
        /// </returns>
        protected abstract IDbConnection NewConnection(string connectionString);

        /// <summary>
        ///     The query.
        /// </summary>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <param name="query">
        ///     The query.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="IEnumerable" />.
        /// </returns>
        public virtual IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query) {
            if (this.SelectWriter == null) {
                throw new Exception("The SelectWriter has not been initialised");
            }

            var sqlQuery = this.SelectWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Query(sqlQuery, query, connection);
        }

        /// <summary>
        ///     The execute.
        /// </summary>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <param name="query">
        ///     The query.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public virtual int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query) {
            if (this.InsertWriter == null) {
                throw new Exception("The InsertWriter has not been initialised");
            }

            var sqlQuery = this.InsertWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }

        /// <summary>
        ///     The execute.
        /// </summary>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <param name="query">
        ///     The query.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public virtual int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query) {
            if (this.UpdateWriter == null) {
                throw new Exception("The UpdateWriter has not been initialised");
            }

            var sqlQuery = this.UpdateWriter.GenerateSql(query);
            if (sqlQuery.Sql.Length > 0) {
                return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
            }

            return 0;
        }

        /// <summary>
        ///     The execute.
        /// </summary>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <param name="query">
        ///     The query.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public virtual int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query) {
            if (this.DeleteWriter == null) {
                throw new Exception("The DeleteWriter has not been initialised");
            }

            var sqlQuery = this.DeleteWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }
    }
}