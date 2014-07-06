namespace Dashing {
    using System.Collections.Generic;

    /// <summary>
    ///     The update entity query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class UpdateEntityQuery<T> : EntityQueryBase<T> {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateEntityQuery{T}" /> class.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        public UpdateEntityQuery(params T[] entities)
            : base(entities) {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateEntityQuery{T}" /> class.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        public UpdateEntityQuery(IEnumerable<T> entities)
            : base(entities) {
        }
    }
}