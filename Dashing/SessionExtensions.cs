namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

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

        public static int InsertOrUpdate<T>(
            this ISession session,
            T entity,
            Expression<Func<T, bool>> equalityComparer = null) {

            if (equalityComparer == null) {
                // if the equality comparer is null then they should be passing us a valid PK value in the entity so call update
                var updated = session.Save(entity);
                if (updated == 0) {
                    return session.Insert(entity);
                }

                return updated;
            }
            else {
                // for seeding the users identity is based on something else
                var existingEntity = session.Query<T>().SingleOrDefault(equalityComparer);
                var map = session.Configuration.GetMap<T>();
                foreach (var col in map.OwnedColumns()) {
                    // map over to existing entity
                    map.SetColumnValue(existingEntity, col, map.GetColumnValue(entity, col));
                }

                return session.Save(existingEntity);
            }
        }
    }
}