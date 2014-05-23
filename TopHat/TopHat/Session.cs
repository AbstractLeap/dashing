namespace TopHat {
  using System;
  using System.Collections.Generic;
  using System.Data;

  /// <summary>
  ///   The session.
  /// </summary>
  public sealed class Session : ISession {
    /// <summary>
    ///   The _engine.
    /// </summary>
    private readonly IEngine _engine;

    /// <summary>
    ///   The _connection.
    /// </summary>
    private readonly IDbConnection _connection;

    /// <summary>
    ///   The _is their transaction.
    /// </summary>
    private readonly bool _isTheirTransaction;

    /// <summary>
    ///   The _transaction.
    /// </summary>
    private IDbTransaction _transaction;

    /// <summary>
    ///   The _is disposed.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    ///   The _is completed.
    /// </summary>
    private bool _isCompleted;

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

      this._engine = engine;
      this._connection = connection;
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

      this._engine = engine;
      this._connection = connection;

      if (transaction != null) {
        this._isTheirTransaction = true;
        this._transaction = transaction;
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
        if (this._isDisposed) {
          throw new ObjectDisposedException("Session");
        }

        if (this._connection.State == ConnectionState.Closed) {
          this._connection.Open();
        }

        if (this._connection.State == ConnectionState.Open) {
          return this._connection;
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
        if (this._isDisposed) {
          throw new ObjectDisposedException("Session");
        }

        return this._transaction ?? (this._transaction = this.Connection.BeginTransaction());
      }
    }

    /// <summary>
    ///   The complete.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// </exception>
    public void Complete() {
      if (this._isCompleted) {
        throw new InvalidOperationException("Only call complete once, when all of the transactional work is done");
      }

      if (this._transaction != null && !this._isTheirTransaction) {
        this._transaction.Commit();
      }

      this._isCompleted = true;
    }

    /// <summary>
    ///   The dispose.
    /// </summary>
    public void Dispose() {
      if (this._isDisposed) {
        return;
      }

      if (this._transaction != null && !this._isTheirTransaction) {
        if (!this._isCompleted) {
          this._transaction.Rollback();
        }

        this._transaction.Dispose();
      }

      this._isDisposed = true;
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
      return new ExecutableSelectQuery<T>(this._engine, this.Connection);
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
      return this._engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
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
      return this._engine.Execute(this.Connection, new InsertEntityQuery<T>(entities));
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
      return this._engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
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
      return this._engine.Execute(this.Connection, new UpdateEntityQuery<T>(entities));
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
      return this._engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
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
      return this._engine.Execute(this.Connection, new DeleteEntityQuery<T>(entities));
    }
  }
}