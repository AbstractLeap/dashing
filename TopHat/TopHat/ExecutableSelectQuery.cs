namespace TopHat {
  using System.Collections;
  using System.Collections.Generic;
  using System.Data;

  /// <summary>
  ///   The executable select query.
  /// </summary>
  /// <typeparam name="T">
  /// </typeparam>
  public class ExecutableSelectQuery<T> : SelectQuery<T>, IEnumerable<T> {
    /// <summary>
    ///   The _engine.
    /// </summary>
    private readonly IEngine _engine;

    /// <summary>
    ///   The _connection.
    /// </summary>
    private readonly IDbConnection _connection;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ExecutableSelectQuery{T}" /> class.
    /// </summary>
    /// <param name="engine">
    ///   The engine.
    /// </param>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    public ExecutableSelectQuery(IEngine engine, IDbConnection connection) {
      this._connection = connection;
      this._engine = engine;
    }

    /// <summary>
    ///   The get enumerator.
    /// </summary>
    /// <returns>
    ///   The <see cref="IEnumerator" />.
    /// </returns>
    public IEnumerator<T> GetEnumerator() {
      return this._engine.Query(this._connection, this).GetEnumerator();
    }

    /// <summary>
    ///   The get enumerator.
    /// </summary>
    /// <returns>
    ///   The <see cref="IEnumerator" />.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }
  }
}