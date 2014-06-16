namespace TopHat {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;

    using TopHat.Engine;

    /// <summary>
    ///     The select query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class SelectQuery<T> : ISelectQuery<T> {
        private readonly IEngine engine;

        private readonly IDbConnection connection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectQuery{T}" /> class.
        /// </summary>
        public SelectQuery(IEngine engine, IDbConnection connection) {
            this.engine = engine;

            this.connection = connection;
            this.Includes = new List<Expression>();
            this.Excludes = new List<Expression>();
            this.Fetches = new List<Expression>();
            this.OrderClauses = new Queue<OrderClause<T>>();
            this.WhereClauses = new List<Expression<Func<T, bool>>>();
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
        /// 
        /// </summary>
        public Tuple<Expression, Expression> CollectionFetches { get; set; }

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
            return this.Fetches.Any() || this.CollectionFetches != null;
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
            return this.engine.Query(this.connection, this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public IFetchMany<T, TResult> FetchMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector) {
            return new FetchMany<T, TResult>(selector, this);
        }
    }
}