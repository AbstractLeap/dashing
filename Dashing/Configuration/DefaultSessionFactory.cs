namespace Dashing.Configuration {
    using System.Data;

    /// <summary>
    ///     The default session factory.
    /// </summary>
    public class DefaultSessionFactory : ISessionFactory {
        /// <summary>
        ///     The create.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        public ISession Create(IDbConnection connection, IConfiguration config) {
            return new Session(connection, config);
        }

        /// <summary>
        ///     The create.
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
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        public ISession Create(IDbConnection connection, IDbTransaction transaction, IConfiguration config) {
            return new Session(connection, config, transaction);
        }
    }
}