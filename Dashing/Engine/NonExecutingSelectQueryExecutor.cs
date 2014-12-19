namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class NonExecutingSelectQueryExecutor : ISelectQueryExecutor {
        public IEnumerable<T> Query<T>(SelectQuery<T> query) {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Page<T> QueryPaged<T>(SelectQuery<T> query) {
            throw new NotImplementedException("I don't execute queries!");
        }

        public int Count<T>(SelectQuery<T> query) {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<IEnumerable<T>> QueryAsync<T>(SelectQuery<T> query) {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<Page<T>> QueryPagedAsync<T>(SelectQuery<T> query) {
            throw new NotImplementedException("I don't execute queries!");
        }

        public Task<int> CountAsync<T>(SelectQuery<T> query) {
            throw new NotImplementedException("I don't execute queries!");
        }
    }
}