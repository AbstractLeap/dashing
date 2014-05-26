namespace TopHat {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>
    ///     The select query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class SelectQuery<T> : QueryBase<T> {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelectQuery{T}" /> class.
        /// </summary>
        public SelectQuery() {
            this.Includes = new List<Expression>();
            this.Excludes = new List<Expression>();
            this.Fetches = new List<Expression>();
            this.OrderClauses = new Queue<OrderClause<T>>();
        }

        /// <summary>
        ///     Gets or sets the projection.
        /// </summary>
        public Expression<Func<T, dynamic>> Projection { get; set; }

        /// <summary>
        ///     Gets or sets the includes.
        /// </summary>
        public List<Expression> Includes { get; set; }

        /// <summary>
        ///     Gets or sets the excludes.
        /// </summary>
        public List<Expression> Excludes { get; set; }

        /// <summary>
        ///     Gets or sets the fetches.
        /// </summary>
        public List<Expression> Fetches { get; set; }

        /// <summary>
        ///     Gets or sets the order clauses.
        /// </summary>
        public Queue<OrderClause<T>> OrderClauses { get; set; }

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

        /// <summary>
        ///     The where.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate.
        /// </param>
        /// <remarks>
        ///     We override the base here to keep the upgraded interface offered by SelectQuery`T
        /// </remarks>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public new SelectQuery<T> Where(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this;
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
        public SelectQuery<T> Select(Expression<Func<T, dynamic>> projection) {
            this.Projection = projection;
            return this;
        }

        /// <summary>
        ///     The include all.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public SelectQuery<T> IncludeAll() {
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
        public SelectQuery<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression) {
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
        public SelectQuery<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression) {
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
        public SelectQuery<T> Fetch<TFetch>(Expression<Func<T, TFetch>> selector) {
            this.Fetches.Add(selector);
            return this;
        }

        /// <summary>
        ///     The for update.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public SelectQuery<T> ForUpdate() {
            this.IsForUpdate = true;
            return this;
        }

        /// <summary>
        ///     The as tracked.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery" />.
        /// </returns>
        public SelectQuery<T> AsTracked() {
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
        public SelectQuery<T> Skip(int skip) {
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
        public SelectQuery<T> Take(int take) {
            this.TakeN = take;
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
        public SelectQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector) {
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
        public SelectQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Enqueue(new OrderClause<T>(keySelector, ListSortDirection.Descending));
            return this;
        }
    }
}