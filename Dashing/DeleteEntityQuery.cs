namespace Dashing {
    using System.Collections.Generic;

    /// <summary>
    ///     The delete entity query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class DeleteEntityQuery<T> : EntityQueryBase<T> {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeleteEntityQuery{T}" /> class.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        public DeleteEntityQuery(params T[] entities)
            : base(entities) {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeleteEntityQuery{T}" /> class.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        public DeleteEntityQuery(IEnumerable<T> entities)
            : base(entities) {
        }
    }
}