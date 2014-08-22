namespace Dashing.Configuration {
    using System;

    /// <summary>
    ///     The Mapper interface.
    /// </summary>
    public interface IMapper {
        /// <summary>
        ///     Return a typed map for the typeparameter specified
        /// </summary>
        /// <typeparam name="T">Type to be mapped</typeparam>
        /// <param name="configuration">Configuration that the map belongs to</param>
        /// <returns>Map for the type</returns>
        IMap<T> MapFor<T>(IConfiguration configuration);

        /// <summary>
        ///     Return a generic map for the type specified
        /// </summary>
        /// <param name="type">Type to be mapped</param>
        /// <param name="configuration">Configuration that the map belongs to</param>
        /// <returns>Map for the type</returns>
        IMap MapFor(Type type, IConfiguration configuration);
    }
}