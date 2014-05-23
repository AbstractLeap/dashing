namespace TopHat.Configuration {
  using System.Data;

  /// <summary>
  ///   The SessionFactory interface.
  /// </summary>
  public interface ISessionFactory {
    /// <summary>
    ///   The create.
    /// </summary>
    /// <param name="engine">
    ///   The engine.
    /// </param>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <returns>
    ///   The <see cref="ISession" />.
    /// </returns>
    ISession Create(IEngine engine, IDbConnection connection);

    /// <summary>
    ///   The create.
    /// </summary>
    /// <param name="engine">
    ///   The engine.
    /// </param>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <param name="transaction">
    ///   The transaction.
    /// </param>
    /// <returns>
    ///   The <see cref="ISession" />.
    /// </returns>
    ISession Create(IEngine engine, IDbConnection connection, IDbTransaction transaction);
  }
}