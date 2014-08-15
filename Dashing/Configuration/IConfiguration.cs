namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Engine;
using Dashing.Events;

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
        IMap<T> GetMap<T>();

        /// <summary>
        ///     Returns the map for a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IMap GetMap(Type type);

        /// <summary>
        ///     Returns whether the configuration contains a map for a particular type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasMap(Type type);

        ISession BeginSession();

        ISession BeginSession(IDbConnection connection);

        ISession BeginSession(IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        ///     The code manager for this configuration
        /// </summary>
        /// <returns></returns>
        IGeneratedCodeManager CodeManager { get; }

        ICollection<IEventListener> EventListeners { get; }

        EventHandlers EventHandlers { get; }

        /// <summary>
        ///     The engine for this configuration
        /// </summary>
        /// <returns></returns>
        IEngine Engine { get; }

        /// <summary>
        ///     The mapper for this configuration
        /// </summary>
        /// <returns></returns>
        IMapper Mapper { get; }

        /// <summary>
        ///     Indicates whether Get method calls return tracked entities by default or not
        /// </summary>
        bool GetIsTrackedByDefault { get; set; }
    }
}