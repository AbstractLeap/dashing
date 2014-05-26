namespace TopHat.Configuration {
  using System.Data;

  using TopHat.Engine;

  /// <summary>
  ///   The default session factory.
  /// </summary>
  public class DefaultSessionFactory : ISessionFactory {
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
    public ISession Create(IEngine engine, IDbConnection connection) {
      return new Session(engine, connection);
    }

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
    public ISession Create(IEngine engine, IDbConnection connection, IDbTransaction transaction) {
      return new Session(engine, connection, transaction);
    }
  }
}