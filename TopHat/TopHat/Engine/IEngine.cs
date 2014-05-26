namespace TopHat.Engine {
  using System;
  using System.Collections.Generic;
  using System.Data;

  using TopHat.Configuration;

  /// <summary>
  ///   The Engine interface.
  /// </summary>
  public interface IEngine {
    /// <summary>
    ///   The open.
    /// </summary>
    /// <param name="connectionString">
    ///   The connection string.
    /// </param>
    /// <returns>
    ///   The <see cref="IDbConnection" />.
    /// </returns>
    IDbConnection Open(string connectionString);

    /// <summary>
    ///   The use maps.
    /// </summary>
    /// <param name="maps">
    ///   The maps.
    /// </param>
    void UseMaps(IDictionary<Type, IMap> maps);

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
    IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query);

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
    int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query);

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
    int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query);

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
    int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query);
  }
}