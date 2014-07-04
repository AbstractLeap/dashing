namespace Dashing.Configuration {
    /// <summary>
    ///     The relationship type.
    /// </summary>
    public enum RelationshipType {
        /// <summary>
        ///     The none.
        /// </summary>
        None, 

        /// <summary>
        ///     The one to many.
        /// </summary>
        OneToMany, 

        /// <summary>
        ///     The many to one.
        /// </summary>
        ManyToOne, 

        /// <summary>
        ///     The many to many.
        /// </summary>
        ManyToMany
    }
}