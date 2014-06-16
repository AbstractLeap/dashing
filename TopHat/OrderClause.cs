namespace TopHat {
    using System.ComponentModel;
    using System.Linq.Expressions;

    /// <summary>
    ///     The order clause.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class OrderClause<T> {
        /// <summary>
        ///     Gets the expression.
        /// </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        ///     Gets the direction.
        /// </summary>
        public ListSortDirection Direction { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderClause{T}" /> class.
        /// </summary>
        /// <param name="expression">
        ///     The expression.
        /// </param>
        /// <param name="direction">
        ///     The direction.
        /// </param>
        public OrderClause(Expression expression, ListSortDirection direction) {
            this.Expression = expression;
            this.Direction = direction;
        }
    }
}