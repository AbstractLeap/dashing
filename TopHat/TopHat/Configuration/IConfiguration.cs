namespace TopHat.Configuration {
  using System.Collections.Generic;

  /// <summary>
  ///   The Configuration interface.
  /// </summary>
  public interface IConfiguration {
    /// <summary>
    ///   Gets the maps.
    /// </summary>
    IEnumerable<IMap> Maps { get; }

    /// <summary>
    ///   The begin session.
    /// </summary>
    /// <returns>
    ///   The <see cref="ISession" />.
    /// </returns>
    ISession BeginSession();
  }
}