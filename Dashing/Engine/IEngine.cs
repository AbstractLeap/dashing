namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    /// <summary>
    ///     The Engine interface.
    /// </summary>
    public interface IEngine {
        /// <summary>
        ///     The open.
        /// </summary>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        /// <returns>
        ///     The <see cref="IDbConnection" />.
        /// </returns>
        IDbConnection Open(string connectionString);

        /// <summary>
        ///     Get set the current configuration for this engine
        /// </summary>
        IConfiguration Configuration { get; set; }

        /// <summary>
        ///     The use maps.
        /// </summary>
        /// <param name="maps">
        ///     The maps.
        /// </param>
        void UseMaps(IDictionary<Type, IMap> maps);

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
        IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query);

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
        int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query);

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
        int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query);

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
        int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query);

        T Get<T>(IDbConnection connection, int id, bool? asTracked);

        T Get<T>(IDbConnection connection, Guid id, bool? asTracked);

        IEnumerable<T> Get<T>(IDbConnection connection, IEnumerable<int> ids, bool? asTracked);

        IEnumerable<T> Get<T>(IDbConnection connection, IEnumerable<Guid> ids, bool? asTracked);

        void Execute<T>(IDbConnection dbConnection, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        void ExecuteBulkDelete<T>(IDbConnection connection, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}