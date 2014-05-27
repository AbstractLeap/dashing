namespace TopHat.Configuration {
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The Configuration interface.
    /// </summary>
    public interface IConfiguration {
        /// <summary>
        ///     Gets the maps.
        /// </summary>
        IEnumerable<IMap> Maps { get; }

        /// <summary>
        ///     Returns the map for a particular type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IMap GetMap<T>();

        /// <summary>
        ///     Returns the map for a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IMap GetMap(Type type);

        /// <summary>
        ///     The begin session.
        /// </summary>
        /// <returns>
        ///     The <see cref="ISession" />.
        /// </returns>
        ISession BeginSession();
    }
}