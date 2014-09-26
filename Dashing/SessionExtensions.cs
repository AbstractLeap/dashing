namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;

    public static class SessionExtensions {
        public static T Get<T>(this ISession session, int id) {
            return session.Get<T, int>(id);
        }

        public static T Get<T>(this ISession session, Guid id) {
            return session.Get<T, Guid>(id);
        }

        public static IEnumerable<T> Get<T>(this ISession session, params int[] ids) {
            return session.Get<T>(ids as IEnumerable<int>);
        }

        public static IEnumerable<T> Get<T>(this ISession session, IEnumerable<int> ids) {
            return session.Get<T, int>(ids);
        }

        public static IEnumerable<T> Get<T>(this ISession session, params Guid[] ids) {
            return session.Get<T>(ids as IEnumerable<Guid>);
        }

        public static IEnumerable<T> Get<T>(this ISession session, IEnumerable<Guid> ids) {
            return session.Get<T, Guid>(ids);
        }

        public static T GetTracked<T>(this ISession session, int id) {
            return session.GetTracked<T, int>(id);
        }

        public static T GetTracked<T>(this ISession session, Guid id) {
            return session.GetTracked<T, Guid>(id);
        }

        public static IEnumerable<T> GetTracked<T>(this ISession session, params int[] ids) {
            return session.GetTracked<T>(ids as IEnumerable<int>);
        }

        public static IEnumerable<T> GetTracked<T>(this ISession session, IEnumerable<int> ids) {
            return session.GetTracked<T, int>(ids);
        }

        public static IEnumerable<T> GetTracked<T>(this ISession session, params Guid[] ids) {
            return session.GetTracked<T>(ids as IEnumerable<Guid>);
        }

        public static IEnumerable<T> GetTracked<T>(this ISession session, IEnumerable<Guid> ids) {
            return session.GetTracked<T, Guid>(ids);
        }

        public static int Insert<T>(this ISession session, params T[] entities) {
            return session.Insert(entities);
        }

        public static int Save<T>(this ISession session, params T[] entities) {
            return session.Save(entities);
        }

        public static int Delete<T>(this ISession session, params T[] entities) {
            return session.Delete(entities);
        }

        public static int Update<T>(this ISession session, Action<T> update, params Expression<Func<T, bool>>[] predicates) {
            return session.Update(update, predicates);
        }

        public static int Delete<T>(this ISession session, params Expression<Func<T, bool>>[] predicates) {
            return session.Delete(predicates);
        }

        public static int InsertOrUpdate<T>(this ISession session, T entity, Expression<Func<T, bool>> equalityComparer = null) where T : class {
            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = session.Save(entity);
                return updated == 0 ? session.Insert(entity) : updated;
            }
            
            // we support different equalityComparers so we can cope with e.g. username 
            var existingEntity = session.Query<T>().AsTracked().SingleOrDefault(equalityComparer);
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
        public static async Task<T> GetAsync<T>(this ISession session, int id) {
            return await session.GetAsync<T, int>(id);
        }

        public static async Task<T> GetAsync<T>(this ISession session, Guid id) {
            return await session.GetAsync<T, Guid>(id);
        }

        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, params int[] ids) {
            return await session.GetAsync<T>(ids as IEnumerable<int>);
        }

        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, IEnumerable<int> ids) {
            return await session.GetAsync<T, int>(ids);
        }

        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, params Guid[] ids) {
            return await session.GetAsync<T>(ids as IEnumerable<Guid>);
        }

        public static async Task<IEnumerable<T>> GetAsync<T>(this ISession session, IEnumerable<Guid> ids) {
            return await session.GetAsync<T, Guid>(ids);
        }

        public static async Task<T> GetTrackedAsync<T>(this ISession session, int id) {
            return await session.GetTrackedAsync<T, int>(id);
        }

        public static async Task<T> GetTrackedAsync<T>(this ISession session, Guid id) {
            return await session.GetTrackedAsync<T, Guid>(id);
        }

        public static async Task<IEnumerable<T>> GetTrackedAsync<T>(this ISession session, params int[] ids) {
            return await session.GetTrackedAsync<T>(ids as IEnumerable<int>);
        }

        public static async Task<IEnumerable<T>> GetTrackedAsync<T>(this ISession session, IEnumerable<int> ids) {
            return await session.GetTrackedAsync<T, int>(ids);
        }

        public static async Task<IEnumerable<T>> GetTrackedAsync<T>(this ISession session, params Guid[] ids) {
            return await session.GetTrackedAsync<T>(ids as IEnumerable<Guid>);
        }

        public static async Task<IEnumerable<T>> GetTrackedAsync<T>(this ISession session, IEnumerable<Guid> ids) {
            return await session.GetTrackedAsync<T, Guid>(ids);
        }

        public static async Task<int> InsertAsync<T>(this ISession session, params T[] entities) {
            return await session.InsertAsync(entities);
        }

        public static async Task<int> SaveAsync<T>(this ISession session, params T[] entities) {
            return await session.SaveAsync(entities);
        }

        public static async Task<int> DeleteAsync<T>(this ISession session, params T[] entities) {
            return await session.DeleteAsync(entities);
        }

        public static async Task<int> UpdateAsync<T>(this ISession session, Action<T> update, params Expression<Func<T, bool>>[] predicates) {
            return await session.UpdateAsync(update, predicates);
        }

        public static async Task<int> DeleteAsync<T>(this ISession session, params Expression<Func<T, bool>>[] predicates) {
            return await session.DeleteAsync(predicates);
        }

        public static async Task<int> InsertOrUpdateAsync<T>(this ISession session, T entity, Expression<Func<T, bool>> equalityComparer = null) where T : class {
            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = await session.SaveAsync(entity);
                return updated == 0 ? await session.InsertAsync(entity) : updated;
            }

            // we support different equalityComparers so we can cope with e.g. username 
            var existingEntity = await session.Query<T>().AsTracked().SingleOrDefaultAsync(equalityComparer);
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
    }
}