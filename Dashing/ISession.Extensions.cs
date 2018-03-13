namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;

    public partial interface ISession {
        /// <summary>
        ///     Get an entity by Int64 primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        T Get<T>(long id)
            where T : class, new();

        /// <summary>
        ///     Get an entity by integer primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        T Get<T>(int id)
            where T : class, new();

        /// <summary>
        ///     Get an entity by Guid primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        T Get<T>(Guid id)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their Int64 primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(params long[] ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their Int64 primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(IEnumerable<long> ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(params int[] ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(IEnumerable<int> ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(params Guid[] ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(IEnumerable<Guid> ids)
            where T : class, new();

        /// <summary>
        ///     Inserts a collection of entities in to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <remarks>Where the primary key is dynamically generated the primary key will be populated</remarks>
        int Insert<T>(params T[] entities)
            where T : class, new();

        /// <summary>
        ///     Saves all changes on a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Save<T>(params T[] entities)
            where T : class, new();

        /// <summary>
        ///     Deletes a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Delete<T>(params T[] entities)
            where T : class, new();

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
        int Update<T>(Action<T> update, params Expression<Func<T, bool>>[] predicates)
            where T : class, new();

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
        int Delete<T>(params Expression<Func<T, bool>>[] predicates)
            where T : class, new();

        /// <summary>
        ///     Inserts or updates a particular entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="equalityComparer">Indicates how to compare whether two entities are equal</param>
        /// <returns></returns>
        /// <remarks>
        ///     If you do not specify an equalityComparer this function will simply attempt a Save then an Insert. If you do
        ///     provide an equalityComparer this will fetch the entity and then update it
        /// </remarks>
        int InsertOrUpdate<T>(T entity, Expression<Func<T, bool>> equalityComparer = null)
            where T : class, new();

        /// <summary>
        ///     Get an entity by long primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(long id) where T : class, new();

        /// <summary>
        ///     Get an entity by integer primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(int id)
            where T : class, new();

        /// <summary>
        ///     Get an entity by Guid primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(Guid id)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their long primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(params long[] ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their long primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(IEnumerable<long> ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(params int[] ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(IEnumerable<int> ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(params Guid[] ids)
            where T : class, new();

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(IEnumerable<Guid> ids)
            where T : class, new();

        /// <summary>
        ///     Inserts a collection of entities in to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <remarks>Where the primary key is dynamically generated the primary key will be populated</remarks>
        Task<int> InsertAsync<T>(params T[] entities)
            where T : class, new();

        /// <summary>
        ///     Saves all changes on a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> SaveAsync<T>(params T[] entities)
            where T : class, new();

        /// <summary>
        ///     Deletes a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<int> DeleteAsync<T>(params T[] entities)
            where T : class, new();

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
        Task<int> UpdateAsync<T>(Action<T> update, params Expression<Func<T, bool>>[] predicates)
            where T : class, new();

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
        Task<int> DeleteAsync<T>(params Expression<Func<T, bool>>[] predicates)
            where T : class, new();

        /// <summary>
        ///     Inserts or updates a particular entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="equalityComparer">Indicates how to compare whether two entities are equal</param>
        /// <returns></returns>
        /// <remarks>
        ///     If you do not specify an equalityComparer this function will simply attempt a Save then an Insert. If you do
        ///     provide an equalityComparer this will fetch the entity and then update it
        /// </remarks>
        Task<int> InsertOrUpdateAsync<T>(T entity, Expression<Func<T, bool>> equalityComparer = null)
            where T : class, new();

        /// <summary>
        ///     Casts the entity to an ITrackedEntity for inspecting the changes since EnableTracking was called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        ITrackedEntityInspector<T> Inspect<T>(T entity);
    }
}