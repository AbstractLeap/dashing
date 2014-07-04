namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    ///     The query base.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class QueryBase<T> {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryBase{T}" /> class.
        /// </summary>
        public QueryBase() {
            this.WhereClauses = new List<Expression<Func<T, bool>>>();
        }

        /// <summary>
        ///     Gets or sets the where clauses.
        /// </summary>
        public IList<Expression<Func<T, bool>>> WhereClauses { get; set; }

        /// <summary>
        ///     The where.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate.
        /// </param>
        /// <returns>
        ///     The <see cref="QueryBase" />.
        /// </returns>
        public QueryBase<T> Where(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this;
        }
    }
}