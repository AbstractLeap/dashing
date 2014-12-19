namespace Dashing {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISelectQueryExecutor {
        IEnumerable<T> Query<T>(SelectQuery<T> query);

        Page<T> QueryPaged<T>(SelectQuery<T> query);

        int Count<T>(SelectQuery<T> query);

        Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query);

        Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query);

        Task<int> CountAsync<T>(SelectQuery<T> query);
    }
}