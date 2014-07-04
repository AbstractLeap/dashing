namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    ///     The update query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class UpdateQuery<T> : QueryBase<T> {
        /// <summary>
        ///     Gets or sets the assignments.
        /// </summary>
        public List<Expression<Action<T>>> Assignments { get; set; }
    }
}