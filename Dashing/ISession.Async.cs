namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public partial interface ISession {
        /// <summary>
        ///     Get an entity by primary key
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key</typeparam>
        /// <param name="id">The primary key of the entity to return</param>
        /// <returns></returns>
        Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id)
            where T : class, new();

        /// <summary>
        ///     Get an enumerable of entities by their primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key</typeparam>
        /// <param name="ids">The primary keys of the entities you wish to return</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids)
            where T : class, new();

        /// <summary>
        ///     Inserts an entity or a collection of entities in to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <remarks>Where the primary key is dynamically generated the primary key will be populated</remarks>
        Task<int> InsertAsync<T>(T entities);

        /// <summary>
        ///     Saves all changes on an entity or a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> SaveAsync<T>(T entities);

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
        Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        /// <summary>
        ///     Deletes an entity or a collection of entities
        /// </summary>
        /// <returns></returns>
        Task<int> DeleteAsync<T>(T entities);

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
        Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        /// <summary>
        ///     Updates all entities of a particular type
        /// </summary>
        /// <typeparam name="T">The type of entities to update</typeparam>
        /// <param name="update">The updates you wish to perform against the entities</param>
        /// <returns></returns>
        /// <remarks>This performs an UPDATE query with no where clause. Use with caution!</remarks>
        Task<int> UpdateAllAsync<T>(Action<T> update)
            where T : class, new();

        /// <summary>
        ///     Deletes all entities of a particular type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>This performs a DELETE query with no where clause. Use with caution!</remarks>
        Task<int> DeleteAllAsync<T>()
            where T : class, new();
    }
}