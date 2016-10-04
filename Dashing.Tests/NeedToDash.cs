namespace Dashing.Tests {
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using Dashing.Configuration;

    /// <summary>
    ///     Provides methods to configure Dashing in a single statement.
    /// </summary>
    public static class NeedToDash {
        /// <summary>
        ///     Returns a Configuration object for the connection string that can be fluently configured
        /// </summary>
        /// <param name="connectionStringName">name of the connection string to be used</param>
        /// <returns>Mutable configuration object</returns>
        public static MutableConfiguration Configure(string connectionStringName) {
            if (connectionStringName == null) {
                throw new ArgumentNullException("connectionStringName");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null) {
                throw new ArgumentException(string.Format("No connection string was found with the name '{0}'", connectionStringName));
            }

            return Configure(connectionStringSettings);
        }

        /// <summary>
        ///     Returns a Configuration object for the connection string that can be fluently configured
        /// </summary>
        /// <param name="connectionStringName">name of the connection string to be used</param>
        /// <param name="types">Enumerable of types to be mapped</param>
        /// <returns>Mutable configuration object</returns>
        public static MutableConfiguration Configure(string connectionStringName, IEnumerable<Type> types) {
            if (connectionStringName == null) {
                throw new ArgumentNullException("connectionStringName");
            }
            if (types == null) {
                throw new ArgumentNullException("types");
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null) {
                throw new ArgumentException(string.Format("No connection string was found with the name '{0}'", connectionStringName));
            }

            return Configure(connectionStringSettings, types);
        }

        /// <summary>
        ///     Returns a Configuration object for the connection string that can be fluently configured
        /// </summary>
        /// <param name="connectionStringSettings">connection string settings for the database to be used</param>
        /// <returns>Mutable configuration object</returns>
        public static MutableConfiguration Configure(ConnectionStringSettings connectionStringSettings) {
            if (connectionStringSettings == null) {
                throw new ArgumentNullException("connectionStringSettings");
            }

            return new MutableConfiguration(connectionStringSettings);
        }

        /// <summary>
        ///     Returns a Configuration object for the connection string that can be fluently configured
        /// </summary>
        /// <param name="connectionStringSettings">connection string settings for the database to be used</param>
        /// <param name="types">Enumerable of types to be mapped</param>
        /// <returns>Mutable configuration object</returns>
        public static MutableConfiguration Configure(ConnectionStringSettings connectionStringSettings, IEnumerable<Type> types) {
            if (connectionStringSettings == null) {
                throw new ArgumentNullException("connectionStringSettings");
            }

            var configuration = new MutableConfiguration(connectionStringSettings);
            configuration.Add(types);
            return configuration;
        }
    }
}