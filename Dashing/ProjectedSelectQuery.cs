namespace Dashing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Extensions;

    public class ProjectedSelectQuery<TBase, TProjection> : IProjectedSelectQuery<TBase, TProjection>
        where TBase : class, new() {
        private readonly IProjectedSelectQueryExecutor executor;

        public SelectQuery<TBase> BaseSelectQuery { get; }

        public Expression<Func<TBase, TProjection>> ProjectionExpression { get; }

        public ProjectedSelectQuery(IProjectedSelectQueryExecutor executor, SelectQuery<TBase> baseSelectQuery, Expression<Func<TBase, TProjection>> projectionExpression) {
            this.executor = executor;
            this.BaseSelectQuery = baseSelectQuery;
            this.ProjectionExpression = projectionExpression;
        }

        public IEnumerator<TProjection> GetEnumerator() {
            return this.executor.Query(this)
                       .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public TProjection First() {
            var result = this.FirstOrDefault();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public TProjection FirstOrDefault() {
            this.BaseSelectQuery.Take(1);
            return this.ToList()
                       .FirstOrDefault();
        }

        public TProjection Single() {
            var result = this.SingleOrDefault();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public TProjection SingleOrDefault() {
            return Enumerable.SingleOrDefault(this);
        }

        public TProjection Last() {
            var result = this.LastOrDefault();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public TProjection LastOrDefault() {
            if (this.BaseSelectQuery.OrderClauses.IsEmpty()) {
                throw new InvalidOperationException("You can not request the last item without specifying an order clause");
            }

            // switch order clause direction
            foreach (var clause in this.BaseSelectQuery.OrderClauses) {
                clause.Direction = clause.Direction == ListSortDirection.Ascending
                                       ? ListSortDirection.Descending
                                       : ListSortDirection.Ascending;
            }

            this.BaseSelectQuery.Take(1);
            return this.ToList()
                       .FirstOrDefault();
        }

        public Page<TProjection> AsPaged(int skip, int take) {
            this.BaseSelectQuery.Skip(skip)
                .Take(take);
            return this.executor.QueryPaged(this);
        }

        public async Task<IList<TProjection>> ToListAsync() {
            var result = await this.executor.QueryAsync(this);
            return result.ToList();
        }

        public async Task<TProjection[]> ToArrayAsync() {
            var result = await this.executor.QueryAsync(this);
            return result.ToArray();
        }

        public async Task<TProjection> FirstAsync() {
            var result = await this.FirstOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public async Task<TProjection> FirstOrDefaultAsync() {
            this.BaseSelectQuery.Take(1);
            var result = await this.executor.QueryAsync(this);
            return result.FirstOrDefault();
        }

        public async Task<TProjection> SingleAsync() {
            var result = await this.SingleOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public async Task<TProjection> SingleOrDefaultAsync() {
            var result = await this.executor.QueryAsync(this);
            return result.SingleOrDefault();
        }

        public async Task<TProjection> LastAsync() {
            var result = await this.LastOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public Task<TProjection> LastOrDefaultAsync() {
            if (this.BaseSelectQuery.OrderClauses.IsEmpty()) {
                throw new InvalidOperationException("You can not request the last item without specifying an order clause");
            }

            // switch order clause direction
            foreach (var clause in this.BaseSelectQuery.OrderClauses) {
                clause.Direction = clause.Direction == ListSortDirection.Ascending
                                       ? ListSortDirection.Descending
                                       : ListSortDirection.Ascending;
            }

            this.BaseSelectQuery.Take(1);
            return this.FirstOrDefaultAsync();
        }

        public Task<Page<TProjection>> AsPagedAsync(int skip, int take) {
            this.BaseSelectQuery.Skip(skip)
                .Take(take);
            return this.executor.QueryPagedAsync(this);
        }
    }
}