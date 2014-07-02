namespace TopHat.Configuration {
    using System;

    /// <summary>
    ///     The Mapper interface.
    /// </summary>
    public interface IMapper {
        /// <summary>
        /// Return a typed map for the typeparameter specified
        /// </summary>
        /// <typeparam name="T">Type to be mapped</typeparam>
        /// <returns>Map for the type</returns>
        IMap<T> MapFor<T>();

        /// <summary>
        /// Return a generic map for the type specified
        /// </summary>
        /// <param name="type">Type to be mapped</param>
        /// <returns>Map for the type</returns>
        IMap MapFor(Type type);
    }
}