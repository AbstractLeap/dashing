namespace TopHat
{
    using global::TopHat.Configuration;

    using TopHat.Engine;

  /// <summary>
    ///   The top hat.
    /// </summary>
    public static class TH
    {
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
        public static DefaultConfiguration Configure(IEngine engine, string connectionString)
        {
            return new DefaultConfiguration(engine, connectionString);
        }
    }
}