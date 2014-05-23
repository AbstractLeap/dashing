namespace TopHat.Configuration {
  using System;

  /// <summary>
  ///   The Mapper interface.
  /// </summary>
  public interface IMapper {
    /// <summary>
    ///   The map for.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="Map" />.
    /// </returns>
    Map<T> MapFor<T>();
  }
}