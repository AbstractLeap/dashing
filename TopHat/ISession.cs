namespace TopHat {
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    ///     The Session interface.
    /// </summary>
    public interface ISession : IDisposable {
        /// <summary>
        ///     Gets the connection.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        ///     Gets the transaction.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        ///     The complete.
        /// </summary>
        void Complete();

        /// <summary>
        ///     The query.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        ISelectQuery<T> Query<T>();

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
        int Insert<T>(params T[] entities);

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
        int Insert<T>(IEnumerable<T> entities);

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
        int Update<T>(params T[] entities);

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
        int Update<T>(IEnumerable<T> entities);

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
        int Delete<T>(params T[] entities);

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
        int Delete<T>(IEnumerable<T> entities);
    }
}