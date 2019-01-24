namespace Dashing {
    using System.Threading.Tasks;

    public static class SessionExtensions {
        public static int Insert<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.Insert(entities);
        }

        public static int Save<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.Save(entities);
        }

        public static int Delete<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.Delete(entities);
        }

        public static Task<int> InsertAsync<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.InsertAsync(entities);
        }

        public static Task<int> SaveAsync<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.SaveAsync(entities);
        }

        public static Task<int> DeleteAsync<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.DeleteAsync(entities);
        }
    }
}