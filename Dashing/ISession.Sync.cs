namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public partial interface ISession {
        /// <summary>
        ///     Get an entity by primary key
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key</typeparam>
        /// <param name="id">The primary key of the entity to return</param>
        /// <returns></returns>
        T Get<T, TPrimaryKey>(TPrimaryKey id)
            where T : class, new();

        /// <summary>
        ///     Get an enumerable of entities by their primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key</typeparam>
        /// <param name="ids">The primary keys of the entities you wish to return</param>
        /// <returns></returns>
        IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids)
            where T : class, new();

        /// <summary>
        ///     Perform a query against a type of entity
        /// </summary>
        /// <typeparam name="T">The type of the root entity in the query</typeparam>
        /// <returns></returns>
        ISelectQuery<T> Query<T>()
            where T : class, new();

        /// <summary>
        ///     Inserts an entity or a collection of entities in to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <remarks>Where the primary key is dynamically generated the primary key will be populated</remarks>
        int Insert<T>(T entities);

        /// <summary>
        ///     Saves all changes on an entity or a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Save<T>(T entities);

        /// <summary>
        ///     Execute an update query against a collection of entities as defined by the predicates
        /// </summary>
        /// <typeparam name="T">The type of entities to update</typeparam>
        /// <param name="update">The updates you wish to perform against the entities</param>
        /// <param name="predicates">
        ///     A list of predicates that will be applied to each entity to determine if the entity should be
        ///     updated
        /// </param>
        /// <returns></returns>
        /// <remarks>On a Sql database this writes an UPDATE query and executes it i.e. no data is fetched from the server</remarks>
        int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        /// <summary>
        ///     Deletes an entity or a collection of entities
        /// </summary>
        /// <returns></returns>
        int Delete<T>(T entities);

        /// <summary>
        ///     Deletes a collection of entities based on a group of predicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicates">
        ///     A list of predicates that will be applied to each entity to determine if the entity should be
        ///     updated
        /// </param>
        /// <returns></returns>
        /// <remarks>On a Sql database this writes a DELETE query and executes it i.e. no data is fetched from the server</remarks>
        int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        /// <summary>
        ///     Updates all entities of a particular type
        /// </summary>
        /// <typeparam name="T">The type of entities to update</typeparam>
        /// <param name="update">The updates you wish to perform against the entities</param>
        /// <returns></returns>
        /// <remarks>This performs an UPDATE query with no where clause. Use with caution!</remarks>
        int UpdateAll<T>(Action<T> update)
            where T : class, new();

        /// <summary>
        ///     Deletes all entities of a particular type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>This performs a DELETE query with no where clause. Use with caution!</remarks>
        int DeleteAll<T>()
            where T : class, new();
    }
}