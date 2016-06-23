namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;

    public static class SessionExtensions {
        /// <summary>
        ///     Get an entity by Int64 primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public static T Get<T>(this ISession session, long id) where T : class, new() {
            return session.Get<T, long>(id);
        }

        /// <summary>
        ///     Get an entity by integer primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public static T Get<T>(this ISession session, int id) where T : class, new() {
            return session.Get<T, int>(id);
        }

        /// <summary>
        ///     Get an entity by Guid primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public static T Get<T>(this ISession session, Guid id) where T : class, new() {
            return session.Get<T, Guid>(id);
        }

        /// <summary>
        ///     Get a collection of entities using their Int64 primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this ISession session, params long[] ids) where T : class, new() {
            return session.Get<T>(ids as IEnumerable<long>);
        }

        /// <summary>
        ///     Get a collection of entities using their Int64 primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this ISession session, IEnumerable<long> ids) where T : class, new() {
            return session.Get<T, long>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this ISession session, params int[] ids) where T : class, new() {
            return session.Get<T>(ids as IEnumerable<int>);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this ISession session, IEnumerable<int> ids) where T : class, new() {
            return session.Get<T, int>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this ISession session, params Guid[] ids) where T : class, new() {
            return session.Get<T>(ids as IEnumerable<Guid>);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this ISession session, IEnumerable<Guid> ids) where T : class, new() {
            return session.Get<T, Guid>(ids);
        }

        /// <summary>
        ///     Inserts a collection of entities in to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <remarks>Where the primary key is dynamically generated the primary key will be populated</remarks>
        public static int Insert<T>(this ISession session, params T[] entities) where T : class, new() {
            return session.Insert(entities);
        }

        /// <summary>
        ///     Saves all changes on a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static int Save<T>(this ISession session, params T[] entities) where T : class, new() {
            return session.Save(entities);
        }

        /// <summary>
        ///     Deletes a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static int Delete<T>(this ISession session, params T[] entities) where T : class, new() {
            return session.Delete(entities);
        }

        /// <summary>
        ///     Execute an update query against a collection of entities as defined by the predicates
        /// </summary>
        /// <typeparam name="T">The type of entities to update</typeparam>
        /// <param name="session"></param>
        /// <param name="update">The updates you wish to perform against the entities</param>
        /// <param name="predicates">
        ///     A list of predicates that will be applied to each entity to determine if the entity should be
        ///     updated
        /// </param>
        /// <returns></returns>
        /// <remarks>On a Sql database this writes an UPDATE query and executes it i.e. no data is fetched from the server</remarks>
        public static int Update<T>(this ISession session, Action<T> update, params Expression<Func<T, bool>>[] predicates) where T : class, new() {
            return session.Update(update, predicates);
        }

        /// <summary>
        ///     Deletes a collection of entities based on a group of predicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="predicates">
        ///     A list of predicates that will be applied to each entity to determine if the entity should be
        ///     updated
        /// </param>
        /// <returns></returns>
        /// <remarks>On a Sql database this writes a DELETE query and executes it i.e. no data is fetched from the server</remarks>
        public static int Delete<T>(this ISession session, params Expression<Func<T, bool>>[] predicates) where T : class, new() {
            return session.Delete(predicates);
        }

        /// <summary>
        ///     Inserts or updates a particular entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entity"></param>
        /// <param name="equalityComparer">Indicates how to compare whether two entities are equal</param>
        /// <returns></returns>
        /// <remarks>
        ///     If you do not specify an equalityComparer this function will simply attempt a Save then an Insert. If you do
        ///     provide an equalityComparer this will fetch the entity and then update it
        /// </remarks>
        public static int InsertOrUpdate<T>(this ISession session, T entity, Expression<Func<T, bool>> equalityComparer = null) where T : class, new() {
            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = session.Save(entity);
                return updated == 0 ? session.Insert(entity) : updated;
            }

            // we support different equalityComparers so we can cope with e.g. username 
            var existingEntity = session.Query<T>().FirstOrDefault(equalityComparer);
            if (existingEntity == null) {
                return session.Insert(entity);
            }

            // map the properties on to the existing entity
            var map = session.Configuration.GetMap<T>();
            foreach (var col in map.OwnedColumns().Where(c => !c.IsPrimaryKey)) {
                map.SetColumnValue(existingEntity, col, map.GetColumnValue(entity, col));
            }

            return session.Save(existingEntity);
        }

        /// <summary>
        ///     Get an entity by integer primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this ISession session, int id) where T : class, new() {
            return await session.GetAsync<T, int>(id);
        }

        /// <summary>
        ///     Get an entity by Guid primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this ISession session, Guid id) where T : class, new() {
            return await session.GetAsync<T, Guid>(id);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, params int[] ids) where T : class, new() {
            return await session.GetAsync<T>(ids as IEnumerable<int>);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, IEnumerable<int> ids) where T : class, new() {
            return await session.GetAsync<T, int>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, params Guid[] ids) where T : class, new() {
            return await session.GetAsync<T>(ids as IEnumerable<Guid>);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="session">The Session to use</param>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, IEnumerable<Guid> ids) where T : class, new() {
            return await session.GetAsync<T, Guid>(ids);
        }

        /// <summary>
        ///     Inserts a collection of entities in to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <remarks>Where the primary key is dynamically generated the primary key will be populated</remarks>
        public static async Task<int> InsertAsync<T>(this ISession session, params T[] entities) where T : class, new() {
            return await session.InsertAsync(entities);
        }

        /// <summary>
        ///     Saves all changes on a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static async Task<int> SaveAsync<T>(this ISession session, params T[] entities) where T : class, new() {
            return await session.SaveAsync(entities);
        }

        /// <summary>
        ///     Deletes a collection of entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static async Task<int> DeleteAsync<T>(this ISession session, params T[] entities) where T : class, new() {
            return await session.DeleteAsync(entities);
        }

        /// <summary>
        ///     Execute an update query against a collection of entities as defined by the predicates
        /// </summary>
        /// <typeparam name="T">The type of entities to update</typeparam>
        /// <param name="session"></param>
        /// <param name="update">The updates you wish to perform against the entities</param>
        /// <param name="predicates">
        ///     A list of predicates that will be applied to each entity to determine if the entity should be
        ///     updated
        /// </param>
        /// <returns></returns>
        /// <remarks>On a Sql database this writes an UPDATE query and executes it i.e. no data is fetched from the server</remarks>
        public static async Task<int> UpdateAsync<T>(this ISession session, Action<T> update, params Expression<Func<T, bool>>[] predicates)
            where T : class, new() {
            return await session.UpdateAsync(update, predicates);
        }

        /// <summary>
        ///     Deletes a collection of entities based on a group of predicates
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="predicates">
        ///     A list of predicates that will be applied to each entity to determine if the entity should be
        ///     updated
        /// </param>
        /// <returns></returns>
        /// <remarks>On a Sql database this writes a DELETE query and executes it i.e. no data is fetched from the server</remarks>
        public static async Task<int> DeleteAsync<T>(this ISession session, params Expression<Func<T, bool>>[] predicates) where T : class, new() {
            return await session.DeleteAsync(predicates);
        }

        /// <summary>
        ///     Inserts or updates a particular entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="entity"></param>
        /// <param name="equalityComparer">Indicates how to compare whether two entities are equal</param>
        /// <returns></returns>
        /// <remarks>
        ///     If you do not specify an equalityComparer this function will simply attempt a Save then an Insert. If you do
        ///     provide an equalityComparer this will fetch the entity and then update it
        /// </remarks>
        public static async Task<int> InsertOrUpdateAsync<T>(this ISession session, T entity, Expression<Func<T, bool>> equalityComparer = null)
            where T : class, new() {
            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = await session.SaveAsync(entity);
                return updated == 0 ? await session.InsertAsync(entity) : updated;
            }

            // we support different equalityComparers so we can cope with e.g. username 
            var existingEntity = await session.Query<T>().FirstOrDefaultAsync(equalityComparer);
            if (existingEntity == null) {
                return await session.InsertAsync(entity);
            }

            // map the properties on to the existing entity
            var map = session.Configuration.GetMap<T>();
            foreach (var col in map.OwnedColumns().Where(c => !c.IsPrimaryKey)) {
                map.SetColumnValue(existingEntity, col, map.GetColumnValue(entity, col));
            }

            return await session.SaveAsync(existingEntity);
        }

        /// <summary>
        ///     Casts the entity to an ITrackedEntity for inspecting the changes since EnableTracking was called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ITrackedEntityInspector<T> Inspect<T>(this ISession session, T entity) {
            return (ITrackedEntityInspector<T>)Activator.CreateInstance(typeof(TrackedEntityInspector<>).MakeGenericType(typeof(T)), entity);
        }
    }
}