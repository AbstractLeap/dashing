namespace TopHat {
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using TopHat.Engine;

    /// <summary>
    ///     The executable select query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class ExecutableSelectQuery<T> : SelectQuery<T>, IEnumerable<T> {
        /// <summary>
        ///     The _engine.
        /// </summary>
        private readonly IEngine engine;

        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly IDbConnection connection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExecutableSelectQuery{T}" /> class.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connection">
        ///     The connection.
        /// </param>
        public ExecutableSelectQuery(IEngine engine, IDbConnection connection) {
            this.connection = connection;
            this.engine = engine;
        }

        /// <summary>
        ///     The get enumerator.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerator" />.
        /// </returns>
        public IEnumerator<T> GetEnumerator() {
            return this.engine.Query(this.connection, this).GetEnumerator();
        }

        /// <summary>
        ///     The get enumerator.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerator" />.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}