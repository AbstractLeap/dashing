namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;

    public sealed partial class Session {
        /// <summary>
        ///     Get an entity by Int64 primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public T Get<T>(long id)
            where T : class, new() {
            return this.Get<T, long>(id);
        }

        /// <summary>
        ///     Get an entity by integer primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public T Get<T>(int id)
            where T : class, new() {
            return this.Get<T, int>(id);
        }

        /// <summary>
        ///     Get an entity by Guid primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public T Get<T>(Guid id)
            where T : class, new() {
            return this.Get<T, Guid>(id);
        }

        /// <summary>
        ///     Get a collection of entities using their Int64 primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(params long[] ids)
            where T : class, new() {
            return this.Get<T>(ids as IEnumerable<long>);
        }

        /// <summary>
        ///     Get a collection of entities using their Int64 primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(IEnumerable<long> ids)
            where T : class, new() {
            return this.Get<T, long>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(params int[] ids)
            where T : class, new() {
            return this.Get<T>(ids as IEnumerable<int>);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(IEnumerable<int> ids)
            where T : class, new() {
            return this.Get<T, int>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(params Guid[] ids)
            where T : class, new() {
            return this.Get<T>(ids as IEnumerable<Guid>);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public IEnumerable<T> Get<T>(IEnumerable<Guid> ids)
            where T : class, new() {
            return this.Get<T, Guid>(ids);
        }

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
        public int Update<T>(Action<T> update, params Expression<Func<T, bool>>[] predicates)
            where T : class, new() {
            return this.Update(update, predicates as IEnumerable<Expression<Func<T, bool>>>);
        }

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
        public int Delete<T>(params Expression<Func<T, bool>>[] predicates)
            where T : class, new() {
            return this.Delete(predicates as IEnumerable<Expression<Func<T, bool>>>);
        }

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
        public int InsertOrUpdate<T>(T entity, Expression<Func<T, bool>> equalityComparer = null)
            where T : class, new() {
            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = this.Save(entity);
                return updated == 0 ? this.Insert(entity) : updated;
            }

            // we support different equalityComparers so we can cope with e.g. username 
            var existingEntity = this.Query<T>().FirstOrDefault(equalityComparer);
            if (existingEntity == null) {
                return this.Insert(entity);
            }

            // map the properties on to the existing entity
            var map = this.Configuration.GetMap<T>();
            foreach (var col in map.OwnedColumns().Where(c => !c.IsPrimaryKey && !c.IsComputed)) {
                map.SetColumnValue(existingEntity, col, map.GetColumnValue(entity, col));
            }

            return this.Save(existingEntity);
        }

        /// <summary>
        ///     Get an entity by long primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public Task<T> GetAsync<T>(long id)
            where T : class, new() {
            return this.GetAsync<T, long>(id);
        }

        /// <summary>
        ///     Get an entity by integer primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public Task<T> GetAsync<T>(int id)
            where T : class, new() {
            return this.GetAsync<T, int>(id);
        }

        /// <summary>
        ///     Get an entity by Guid primary key
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="id">The primary key of the entity</param>
        /// <returns></returns>
        public Task<T> GetAsync<T>(Guid id)
            where T : class, new() {
            return this.GetAsync<T, Guid>(id);
        }

        /// <summary>
        ///     Get a collection of entities using their long primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAsync<T>(params long[] ids)
            where T : class, new() {
            return this.GetAsync<T>(ids as IEnumerable<long>);
        }

        /// <summary>
        ///     Get a collection of entities using their long primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<long> ids)
            where T : class, new() {
            return this.GetAsync<T, long>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAsync<T>(params int[] ids)
            where T : class, new() {
            return this.GetAsync<T>(ids as IEnumerable<int>);
        }

        /// <summary>
        ///     Get a collection of entities using their integer primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<int> ids)
            where T : class, new() {
            return this.GetAsync<T, int>(ids);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAsync<T>(params Guid[] ids)
            where T : class, new() {
            return this.GetAsync<T>(ids as IEnumerable<Guid>);
        }

        /// <summary>
        ///     Get a collection of entities using their Guid primary keys
        /// </summary>
        /// <typeparam name="T">The type of entity to get</typeparam>
        /// <param name="ids">The primary keys of the entities</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<Guid> ids)
            where T : class, new() {
            return this.GetAsync<T, Guid>(ids);
        }

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
        public Task<int> UpdateAsync<T>(Action<T> update, params Expression<Func<T, bool>>[] predicates)
            where T : class, new() {
            return this.UpdateAsync(update, predicates as IEnumerable<Expression<Func<T, bool>>>);
        }

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
        public Task<int> DeleteAsync<T>(params Expression<Func<T, bool>>[] predicates)
            where T : class, new() {
            return this.DeleteAsync(predicates as IEnumerable<Expression<Func<T, bool>>>);
        }

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
        public async Task<int> InsertOrUpdateAsync<T>(T entity, Expression<Func<T, bool>> equalityComparer = null)
            where T : class, new() {
            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = await this.SaveAsync(entity);
                return updated == 0 ? await this.InsertAsync(entity) : updated;
            }

            // we support different equalityComparers so we can cope with e.g. username 
            var existingEntity = await this.Query<T>().FirstOrDefaultAsync(equalityComparer);
            if (existingEntity == null) {
                return await this.InsertAsync(entity);
            }

            // map the properties on to the existing entity
            var map = this.Configuration.GetMap<T>();
            foreach (var col in map.OwnedColumns().Where(c => !c.IsPrimaryKey && !c.IsComputed)) {
                map.SetColumnValue(existingEntity, col, map.GetColumnValue(entity, col));
            }

            return await this.SaveAsync(existingEntity);
        }

        /// <summary>
        ///     Casts the entity to an ITrackedEntity for inspecting the changes since EnableTracking was called
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ITrackedEntityInspector<T> Inspect<T>(T entity) {
            // We can't just new up an ITrackedEntityInspector because it has a type constraint to ITrackedEntity
            // We don't want to add that constraint here as that interface is added at compile time
            return (ITrackedEntityInspector<T>)Activator.CreateInstance(typeof(TrackedEntityInspector<>).MakeGenericType(typeof(T)), entity);
        }
    }
}