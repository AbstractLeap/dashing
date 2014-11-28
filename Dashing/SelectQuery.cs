namespace Dashing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Engine;
    using Dashing.Extensions;

    /// <summary>
    ///     The select query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class SelectQuery<T> : ISelectQuery<T> {
        private readonly ISelectQueryExecutor selectQueryExecutorExecutor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectQuery{T}" /> class.
        /// </summary>
        public SelectQuery(ISelectQueryExecutor selectQueryExecutorExecutor) {
            this.selectQueryExecutorExecutor = selectQueryExecutorExecutor;
            this.Includes = new List<Expression>();
            this.Excludes = new List<Expression>();
            this.Fetches = new List<Expression>();
            this.OrderClauses = new Queue<OrderClause<T>>();
            this.WhereClauses = new List<Expression<Func<T, bool>>>();
            this.CollectionFetches = new List<KeyValuePair<Expression, Stack<Expression>>>();
        }

        /// <summary>
        ///     Gets or sets the projection.
        /// </summary>
        public Expression<Func<T, dynamic>> Projection { get; set; }

        /// <summary>
        ///     Gets or sets the includes.
        /// </summary>
        public IList<Expression> Includes { get; set; }

        /// <summary>
        ///     Gets or sets the excludes.
        /// </summary>
        public IList<Expression> Excludes { get; set; }

        /// <summary>
        ///     Gets or sets the fetches.
        /// </summary>
        public IList<Expression> Fetches { get; set; }

        /// <summary>
        /// </summary>
        public IList<KeyValuePair<Expression, Stack<Expression>>> CollectionFetches { get; set; }

        /// <summary>
        ///     Gets or sets the order clauses.
        /// </summary>
        public Queue<OrderClause<T>> OrderClauses { get; set; }

        /// <summary>
        ///     Gets or sets the where clauses.
        /// </summary>
        public IList<Expression<Func<T, bool>>> WhereClauses { get; set; }

        /// <summary>
        ///     Gets or sets the skip n.
        /// </summary>
        public int SkipN { get; set; }

        /// <summary>
        ///     Gets or sets the take n.
        /// </summary>
        public int TakeN { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is for update.
        /// </summary>
        public bool IsForUpdate { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether fetch all properties.
        /// </summary>
        public bool FetchAllProperties { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether is tracked.
        /// </summary>
        public bool IsTracked { get; set; }

        public bool HasFetches() {
            return this.Fetches.Any() || this.CollectionFetches.Any();
        }

        /// <summary>
        ///     The select.
        /// </summary>
        /// <param name="projection">
        ///     The projection.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Select(Expression<Func<T, dynamic>> projection) {
            this.Projection = projection;
            return this;
        }

        /// <summary>
        ///     The include all.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> IncludeAll() {
            this.FetchAllProperties = true;
            return this;
        }

        /// <summary>
        ///     The include.
        /// </summary>
        /// <param name="includeExpression">
        ///     The include expression.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression) {
            this.Includes.Add(includeExpression);
            return this;
        }

        /// <summary>
        ///     The exclude.
        /// </summary>
        /// <param name="excludeExpression">
        ///     The exclude expression.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression) {
            this.Excludes.Add(excludeExpression);
            return this;
        }

        /// <summary>
        ///     The fetch.
        /// </summary>
        /// <param name="selector">
        ///     The selector.
        /// </param>
        /// <typeparam name="TFetch">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Fetch<TFetch>(Expression<Func<T, TFetch>> selector) {
            this.Fetches.Add(selector);
            return this;
        }

        /// <summary>
        ///     The for update.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> ForUpdate() {
            this.IsForUpdate = true;
            return this;
        }

        /// <summary>
        ///     The as tracked.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> AsTracked() {
            this.IsTracked = true;
            return this;
        }

        /// <summary>
        ///     The skip.
        /// </summary>
        /// <param name="skip">
        ///     The skip.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Skip(int skip) {
            this.SkipN = skip;
            return this;
        }

        /// <summary>
        ///     The take.
        /// </summary>
        /// <param name="take">
        ///     The take.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> Take(int take) {
            this.TakeN = take;
            return this;
        }

        /// <summary>
        ///     The where.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate.
        /// </param>
        /// <returns>
        ///     The <see cref="QueryBase" />.
        /// </returns>
        public ISelectQuery<T> Where(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this;
        }

        /// <summary>
        ///     The order by.
        /// </summary>
        /// <param name="keySelector">
        ///     The key selector.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Ascending));
            return this;
        }

        /// <summary>
        ///     The order by descending.
        /// </summary>
        /// <param name="keySelector">
        ///     The key selector.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public ISelectQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Descending));
            return this;
        }

        public IEnumerator<T> GetEnumerator() {
            return this.selectQueryExecutorExecutor.Query(this).GetEnumerator();
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
            this.Take(2); // we need to fetch a least two rows in order for the SingleOrDefault to work
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

        public Page<T> AsPaged(int skip, int take) {
            this.Skip(skip).Take(take);
            return this.selectQueryExecutorExecutor.QueryPaged(this);
        }

        public async Task<IList<T>> ToListAsync() {
            var result = await this.selectQueryExecutorExecutor.QueryAsync(this);
            return result.ToList();
        }

        public async Task<T[]> ToArrayAsync() {
            var result = await this.selectQueryExecutorExecutor.QueryAsync(this);
            return result.ToArray();
        }

        public async Task<T> FirstAsync() {
            var result = await this.FirstOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return await this.FirstAsync();
        }

        public async Task<T> FirstOrDefaultAsync() {
            this.Take(1);
            var result = await this.selectQueryExecutorExecutor.QueryAsync(this);
            return result.FirstOrDefault();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return await this.FirstOrDefaultAsync();
        }

        public async Task<T> SingleAsync() {
            var result = await this.SingleOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public async Task<T> SingleAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return await this.SingleAsync();
        }

        public async Task<T> SingleOrDefaultAsync() {
            this.Take(2);
            var result = await this.selectQueryExecutorExecutor.QueryAsync(this);
            return result.SingleOrDefault();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return await this.SingleOrDefaultAsync();
        }

        public async Task<T> LastAsync() {
            var result = await this.LastOrDefaultAsync();
            if (result == null) {
                throw new InvalidOperationException("The query returned no results");
            }

            return result;
        }

        public async Task<T> LastAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return await this.LastAsync();
        }

        public async Task<T> LastOrDefaultAsync() {
            if (this.OrderClauses.IsEmpty()) {
                throw new InvalidOperationException("You can not request the last item without specifying an order clause");
            }

            // switch order clause direction
            foreach (var clause in this.OrderClauses) {
                clause.Direction = clause.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }

            this.Take(1);
            return await this.FirstOrDefaultAsync();
        }

        public async Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            this.Where(predicate);
            return await this.LastOrDefaultAsync();
        }

        public async Task<Page<T>> AsPagedAsync(int skip, int take) {
            this.Skip(skip).Take(take);
            return await this.selectQueryExecutorExecutor.QueryPagedAsync(this);
        }
    }
}