namespace TopHat.Engine {
  using System;
  using System.Collections.Generic;
  using System.Data;

  using global::TopHat.Configuration;

  /// <summary>
  ///   The engine base.
  /// </summary>
  public abstract class EngineBase : IEngine {
    /// <summary>
    ///   Gets or sets the maps.
    /// </summary>
    protected IDictionary<Type, IMap> Maps { get; set; }

    /// <summary>
    ///   The open.
    /// </summary>
    /// <param name="connectionString">
    ///   The connection string.
    /// </param>
    /// <returns>
    ///   The <see cref="IDbConnection" />.
    /// </returns>
    public IDbConnection Open(string connectionString) {
      var connection = this.NewConnection(connectionString);
      connection.Open();
      return connection;
    }

    /// <summary>
    ///   The use maps.
    /// </summary>
    /// <param name="maps">
    ///   The maps.
    /// </param>
    public void UseMaps(IDictionary<Type, IMap> maps) {
      this.Maps = maps;
    }

    /// <summary>
    ///   The new connection.
    /// </summary>
    /// <param name="connectionString">
    ///   The connection string.
    /// </param>
    /// <returns>
    ///   The <see cref="IDbConnection" />.
    /// </returns>
    protected abstract IDbConnection NewConnection(string connectionString);

    /// <summary>
    ///   The query.
    /// </summary>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <param name="query">
    ///   The query.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="IEnumerable" />.
    /// </returns>
    public abstract IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query);

    /// <summary>
    ///   The execute.
    /// </summary>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <param name="query">
    ///   The query.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public abstract int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query);

    /// <summary>
    ///   The execute.
    /// </summary>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <param name="query">
    ///   The query.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public abstract int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query);

    /// <summary>
    ///   The execute.
    /// </summary>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <param name="query">
    ///   The query.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public abstract int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query);
  }
}