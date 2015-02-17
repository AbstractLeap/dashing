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

        /// <summary>
        /// Begins a new Dashing Session
        /// </summary>
        /// <returns></returns>
        /// <remarks>This will instantiate a new DbConnection and a begin a new DbTransaction when it needs to</remarks>
        ISession BeginSession();

        /// <summary>
        /// Begins a new Dashing Session using the provided connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <remarks>This will begin a new DbTransaction when it needs to</remarks>
        ISession BeginSession(IDbConnection connection);

        /// <summary>
        /// Begins a new Dashing session using the provided connection and transaction
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        /// <remarks>All connection and transaction management should be performed by the caller. Calling Complete() on the Session will do nothing</remarks>
        ISession BeginSession(IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Begins a new transaction-less Session
        /// </summary>
        /// <returns></returns>
        /// <remarks>All queries are executed without an explicit transaction.
        /// A connection is created when needed</remarks>
        ISession BeginTransactionLessSession();

        /// <summary>
        /// Begins a new transaction-less Session using the provided connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        /// <remarks>All queries are executed without an explicit transaction.</remarks>
        ISession BeginTransactionLessSession(IDbConnection connection);

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
        /// Indicates if calling Complete on an ISession that's been rejected should not throw an exception
        /// </summary>
        bool CompleteFailsSilentlyIfRejected { get; set; }
    }
}