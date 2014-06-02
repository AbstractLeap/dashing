namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dapper;

    using TopHat.Configuration;

    /// <summary>
    ///     The engine base.
    /// </summary>
    public abstract class EngineBase : IEngine {
        public IConfiguration Configuration { get; set; }

        protected ISelectWriter SelectWriter { get; set; }

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
            this.SelectWriter = new SelectWriter(this.Dialect, this.Maps);
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
            return this.Configuration.GetCodeManager().Query<T>(sqlQuery, query, connection);
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
        public abstract int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query);

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
        public abstract int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query);

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
        public abstract int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query);
    }
}