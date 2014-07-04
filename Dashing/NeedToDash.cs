namespace Dashing {
    using System.Configuration;
    using Dashing.Configuration;
    using Dashing.Engine;

    /// <summary>
    ///     The top hat.
    /// </summary>
    public static class NeedToDash {
        /// <summary>
        ///     The configure.
        /// </summary>
        /// <param name="engine">
        ///     The engine.
        /// </param>
        /// <param name="connectionString">
        ///     The connection string.
        /// </param>
        /// <returns>
        ///     The <see cref="DefaultConfiguration" />.
        /// </returns>
        public static MutableConfiguration Configure(IEngine engine, string connectionString) {
            return new MutableConfiguration(engine, connectionString);
        }

        public static MutableConfiguration Configure(ConnectionStringSettings connectionString)
        {
            return new MutableConfiguration(connectionString);
        }
    }
}