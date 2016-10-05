namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides methods to configure Dashing in a single statement.
    /// </summary>
    public static class NeedToDash {
        /// <summary>
        ///     Returns a Configuration object for the connection string that can be fluently configured
        /// </summary>
        /// <param name="connectionStringSettings">connection string settings for the database to be used</param>
        /// <returns>Mutable configuration object</returns>
        public static MutableConfiguration Configure() {
            return new MutableConfiguration();
        }

        /// <summary>
        ///     Returns a Configuration object for the connection string that can be fluently configured
        /// </summary>
        /// <param name="connectionStringSettings">connection string settings for the database to be used</param>
        /// <param name="types">Enumerable of types to be mapped</param>
        /// <returns>Mutable configuration object</returns>
        public static MutableConfiguration Configure(IEnumerable<Type> types) {
            var configuration = new MutableConfiguration();
            configuration.Add(types);
            return configuration;
        }
    }
}