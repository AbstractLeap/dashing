using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing {
    public static class SessionExtensions {
        public static int Insert<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.Insert(entities as IEnumerable<T>);
        }

        public static int Save<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.Save(entities as IEnumerable<T>);
        }

        public static int Delete<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.Delete(entities as IEnumerable<T>);
        }

        public static Task<int> InsertAsync<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.InsertAsync(entities as IEnumerable<T>);
        }

        public static Task<int> SaveAsync<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.SaveAsync(entities as IEnumerable<T>);
        }

        public static Task<int> DeleteAsync<T>(this ISession session, params T[] entities)
            where T : class, new() {
            return session.DeleteAsync(entities as IEnumerable<T>);
        }
    }
}
