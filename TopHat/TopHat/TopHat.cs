namespace TopHat {
  using global::TopHat.Configuration;

  /// <summary>
  ///   The top hat.
  /// </summary>
  public static class TopHat {
    /// <summary>
    ///   The configure.
    /// </summary>
    /// <param name="engine">
    ///   The engine.
    /// </param>
    /// <param name="connectionString">
    ///   The connection string.
    /// </param>
    /// <returns>
    ///   The <see cref="DefaultConfiguration" />.
    /// </returns>
    public static DefaultConfiguration Configure(IEngine engine, string connectionString) {
      return new DefaultConfiguration(engine, connectionString);
    }
  }
}