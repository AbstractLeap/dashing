namespace Dashing {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISelectQueryExecutor {
        IEnumerable<T> Query<T>(SelectQuery<T> query) where T : class, new();

        Page<T> QueryPaged<T>(SelectQuery<T> query) where T : class, new();

        int Count<T>(SelectQuery<T> query) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) where T : class, new();

        Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) where T : class, new();

        Task<int> CountAsync<T>(SelectQuery<T> query) where T : class, new();
    }
}