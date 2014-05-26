namespace TopHat {
  using System;
  using System.Collections.Generic;
  using System.Data;

  using TopHat.Engine;

  /// <summary>
  ///   The session.
  /// </summary>
  public sealed class Session : ISession {
    /// <summary>
    ///   The _engine.
    /// </summary>
    private readonly IEngine engine;

    /// <summary>
    ///   The _connection.
    /// </summary>
    private readonly IDbConnection connection;

    /// <summary>
    ///   The _is their transaction.
    /// </summary>
    private readonly bool isTheirTransaction;

    /// <summary>
    ///   The _transaction.
    /// </summary>
    private IDbTransaction transaction;

    /// <summary>
    ///   The _is disposed.
    /// </summary>
    private bool isDisposed;

    /// <summary>
    ///   The _is completed.
    /// </summary>
    private bool isCompleted;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Session" /> class.
    /// </summary>
    /// <param name="engine">
    ///   The engine.
    /// </param>
    /// <param name="connection">
    ///   The connection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// </exception>
    public Session(IEngine engine, IDbConnection connection) {
      if (engine == null) {
        throw new ArgumentNullException("engine");
      }

      if (connection == null) {
        throw new ArgumentNullException("connection");
      }

      this.engine = engine;
      this.connection = connection;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="Session" /> class.
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
    /// <exception cref="ArgumentNullException">
    /// </exception>
    public Session(IEngine engine, IDbConnection connection, IDbTransaction transaction = null) {
      if (engine == null) {
        throw new ArgumentNullException("engine");
      }

      if (connection == null) {
        throw new ArgumentNullException("connection");
      }

      this.engine = engine;
      this.connection = connection;

      if (transaction != null) {
        this.isTheirTransaction = true;
        this.transaction = transaction;
      }
    }

    /// <summary>
    ///   Gets the connection.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// </exception>
    /// <exception cref="Exception">
    /// </exception>
    public IDbConnection Connection {
      get {
        if (this.isDisposed) {
          throw new ObjectDisposedException("Session");
        }

        if (this.connection.State == ConnectionState.Closed) {
          this.connection.Open();
        }

        if (this.connection.State == ConnectionState.Open) {
          return this.connection;
        }

        throw new Exception("Connection in unknown state");
      }
    }

    /// <summary>
    ///   Gets the transaction.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// </exception>
    public IDbTransaction Transaction {
      get {
        if (this.isDisposed) {
          throw new ObjectDisposedException("Session");
        }

        return this.transaction ?? (this.transaction = this.Connection.BeginTransaction());
      }
    }

    /// <summary>
    ///   The complete.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// </exception>
    public void Complete() {
      if (this.isCompleted) {
        throw new InvalidOperationException("Only call complete once, when all of the transactional work is done");
      }

      if (this.transaction != null && !this.isTheirTransaction) {
        this.transaction.Commit();
      }

      this.isCompleted = true;
    }

    /// <summary>
    ///   The dispose.
    /// </summary>
    public void Dispose() {
      if (this.isDisposed) {
        return;
      }

      if (this.transaction != null && !this.isTheirTransaction) {
        if (!this.isCompleted) {
          this.transaction.Rollback();
        }

        this.transaction.Dispose();
      }

      this.isDisposed = true;
    }

    /// <summary>
    ///   The query.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="SelectQuery" />.
    /// </returns>
    public SelectQuery<T> Query<T>() {
      return new ExecutableSelectQuery<T>(this.engine, this.Connection);
    }

    /// <summary>
    ///   The insert.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public int Insert<T>(params T[] entities) {
      return this.engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
    }

    /// <summary>
    ///   The insert.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public int Insert<T>(IEnumerable<T> entities) {
      return this.engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
    }

    /// <summary>
    ///   The update.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public int Update<T>(params T[] entities) {
      return this.engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
    }

    /// <summary>
    ///   The update.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public int Update<T>(IEnumerable<T> entities) {
      return this.engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
    }

    /// <summary>
    ///   The delete.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public int Delete<T>(params T[] entities) {
      return this.engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
    }

    /// <summary>
    ///   The delete.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    /// <typeparam name="T">
    /// </typeparam>
    /// <returns>
    ///   The <see cref="int" />.
    /// </returns>
    public int Delete<T>(IEnumerable<T> entities) {
      return this.engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
    }
  }
}