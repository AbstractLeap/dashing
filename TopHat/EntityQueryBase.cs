namespace TopHat {
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     The entity query base.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public abstract class EntityQueryBase<T> {
        /// <summary>
        ///     Gets or sets the entities.
        /// </summary>
        public List<T> Entities { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityQueryBase{T}" /> class.
        /// </summary>
        /// <param name="entities">
        ///     The entities.
        /// </param>
        protected EntityQueryBase(IEnumerable<T> entities) {
            this.Entities = entities as List<T> ?? entities.ToList();
        }
    }
}