namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class NonExecutingSelectQueryExecutor : IProjectedSelectQueryExecutor {
        public IEnumerable<T> Query<T>(SelectQuery<T> query) where T : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query) where T : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public int Count<T>(SelectQuery<T> query) where T : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) where T : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) where T : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<int> CountAsync<T>(SelectQuery<T> query) where T : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public IEnumerable<TProjection> Query<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Page<TProjection> QueryPaged<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<IEnumerable<TProjection>> QueryAsync<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<Page<TProjection>> QueryPagedAsync<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            throw new NotImplementedException("I don't execute queries!");
        }
    }
}