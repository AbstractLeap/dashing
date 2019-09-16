namespace Dashing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Extensions;

    public class SelectQuery<T> : ISelectQuery<T>
        where T : class, new() {
        private readonly IProjectedSelectQueryExecutor executor;

        public SelectQuery(IProjectedSelectQueryExecutor selectQueryExecutor) {
            this.executor = selectQueryExecutor;
            this.Fetches = new List<Expression>();
            this.OrderClauses = new Queue<OrderClause<T>>();
            this.WhereClauses = new List<Expression<Func<T, bool>>>();
            this.CollectionFetches = new List<KeyValuePair<Expression, List<Expression>>>();
        }

        public IList<Expression> Includes { get; private set; }

        public IList<Expression> Excludes { get; private set; }

        public IList<Expression> Fetches { get; private set; }

        public IList<KeyValuePair<Expression, List<Expression>>> CollectionFetches { get; private set; }

        public Queue<OrderClause<T>> OrderClauses { get; internal set; }

        public IList<Expression<Func<T, bool>>> WhereClauses { get; private set; }

        public int SkipN { get; private set; }

        public int TakeN { get; private set; }

        public bool IsForUpdate { get; private set; }

        public bool FetchAllProperties { get; private set; }

        public bool HasFetches() {
            return this.Fetches.Any() || this.CollectionFetches.Any();
        }

        public IProjectedSelectQuery<T, TProjection> Select<TProjection>(Expression<Func<T, TProjection>> projection) {
            return new ProjectedSelectQuery<T, TProjection>(this.executor, this, projection);
        }

        public ISelectQuery<T> IncludeAll() {
            this.FetchAllProperties = true;
            return this;
        }

        public ISelectQuery<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression) {
            if (this.Includes == null) {
                this.Includes = new List<Expression>();
            }

            this.Includes.Add(includeExpression);
            return this;
        }

        public ISelectQuery<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression) {
            if (this.Excludes == null) {
                this.Excludes = new List<Expression>();
            }

            this.Excludes.Add(excludeExpression);
            return this;
        }

        public ISelectQuery<T> Fetch<TFetch>(Expression<Func<T, TFetch>> selector) {
            this.Fetches.Add(selector);
            return this;
        }

        public ISelectQuery<T> ForUpdate() {
            this.IsForUpdate = true;
            return this;
        }

        public ISelectQuery<T> Skip(int skip) {
            this.SkipN = skip;
            return this;
        }

        public ISelectQuery<T> Take(int take) {
            this.TakeN = take;
            return this;
        }

        public ISelectQuery<T> Where(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this;
        }

        public ISelectQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Ascending));
            return this;
        }

        public ISelectQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Descending));
            return this;
        }

        public IEnumerator<T> GetEnumerator() {
            return this.executor.Query(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public IFetchMany<T, TResult> FetchMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector) {
            return new FetchMany<T, TResult>(selector, this);
        }

        public T First() {
            var result = this.FirstOrDefault();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public T First(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.First();
        }

        public T FirstOrDefault() {
            this.Take(1);
            return this.ToList().FirstOrDefault();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.FirstOrDefault();
        }

        public T Single() {
            var result = this.SingleOrDefault();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public T Single(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.Single();
        }

        public T SingleOrDefault() {
            return Enumerable.SingleOrDefault(this);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.SingleOrDefault();
        }

        public T Last() {
            var result = this.LastOrDefault();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public T Last(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.Last();
        }

        public T LastOrDefault() {
            if (this.OrderClauses.IsEmpty()) {
                throw new InvalidOperationException("You can not request the last item without specifying an order clause");
            }

            // switch order clause direction
            foreach (var clause in this.OrderClauses) {
                clause.Direction = clause.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }

            this.Take(1);
            return this.ToList().FirstOrDefault();
        }

        public T LastOrDefault(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.LastOrDefault();
        }

        public int Count() {
            return this.executor.Count(this);
        }

        public int Count(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.Count();
        }

        public Page<T> AsPaged(int skip, int take) {
            this.Skip(skip).Take(take);
            return this.executor.QueryPaged(this);
        }

        public bool Any() {
            this.Take(1);
            return this.executor.Query(this).Any();
        }

        public bool Any(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.Any();
        }

        public async Task<IList<T>> ToListAsync() {
            var result = await this.executor.QueryAsync(this);
            return result.ToList();
        }

        public async Task<T[]> ToArrayAsync() {
            var result = await this.executor.QueryAsync(this);
            return result.ToArray();
        }

        public async Task<T> FirstAsync() {
            var result = await this.FirstOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.FirstAsync();
        }

        public async Task<T> FirstOrDefaultAsync() {
            this.Take(1);
            var result = await this.executor.QueryAsync(this);
            return result.FirstOrDefault();
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.FirstOrDefaultAsync();
        }

        public async Task<T> SingleAsync() {
            var result = await this.SingleOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public Task<T> SingleAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.SingleAsync();
        }

        public async Task<T> SingleOrDefaultAsync() {
            var result = await this.executor.QueryAsync(this);
            return result.SingleOrDefault();
        }

        public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.SingleOrDefaultAsync();
        }

        public async Task<T> LastAsync() {
            var result = await this.LastOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public Task<T> LastAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.LastAsync();
        }

        public Task<T> LastOrDefaultAsync() {
            if (this.OrderClauses.IsEmpty()) {
                throw new InvalidOperationException("You can not request the last item without specifying an order clause");
            }

            // switch order clause direction
            foreach (var clause in this.OrderClauses) {
                clause.Direction = clause.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }

            this.Take(1);
            return this.FirstOrDefaultAsync();
        }

        public Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.LastOrDefaultAsync();
        }

        public Task<int> CountAsync() {
            return this.executor.CountAsync(this);
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.CountAsync();
        }

        public Task<Page<T>> AsPagedAsync(int skip, int take) {
            this.Skip(skip).Take(take);
            return this.executor.QueryPagedAsync(this);
        }

        public async Task<bool> AnyAsync() {
            this.Take(1);
            return (await this.executor.QueryAsync(this)).Any();
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return this.AnyAsync();
        }
    }
}