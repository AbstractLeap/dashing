using System;
using System.Collections.Generic;
using System.Data;
using TopHat.Configuration;

namespace TopHat {
	public sealed class Session : ISession {
		private readonly IEngine _engine;
		private readonly IQueryFactory _queryFactory;
		private readonly IDbConnection _connection;
		private readonly bool _isTheirTransaction;
		private IDbTransaction _transaction;
		private bool _isDisposed;
		private bool _isCompleted;

		public Session(IEngine engine, IQueryFactory queryFactory, IDbConnection connection) {
			if (engine == null) throw new ArgumentNullException("engine");
			if (queryFactory == null) throw new ArgumentNullException("queryFactory");
			if (connection == null) throw new ArgumentNullException("connection");

			_engine = engine;
			_queryFactory = queryFactory;
			_connection = connection;
		}

		public Session(
			IEngine sqlEngine,
			IQueryFactory queryFactory,
			IDbConnection connection,
			IDbTransaction transaction = null) {
			if (sqlEngine == null) throw new ArgumentNullException("engine");
			if (queryFactory == null) throw new ArgumentNullException("queryFactory");
			if (connection == null) throw new ArgumentNullException("connection");

			_engine = sqlEngine;
			_queryFactory = queryFactory;
			_connection = connection;

			if (transaction != null) {
				_isTheirTransaction = true;
				_transaction = transaction;
			}
		}

		public IDbConnection Connection {
			get {
				if (_isDisposed)
					throw new ObjectDisposedException("Session");

				if (_connection.State == ConnectionState.Closed)
					_connection.Open();

				if (_connection.State == ConnectionState.Open)
					return _connection;

				throw new Exception("Connection in unknown state");
			}
		}

		public IDbTransaction Transaction {
			get {
				if (_isDisposed)
					throw new ObjectDisposedException("Session");

				return _transaction ?? (_transaction = Connection.BeginTransaction());
			}
		}

		public void Complete() {
			if (_isCompleted)
				throw new InvalidOperationException("Only call complete once, when all of the transactional work is done");

			if (_transaction != null && !_isTheirTransaction)
				_transaction.Commit();

			_isCompleted = true;
		}

		public void Dispose() {
			if (_isDisposed)
				return;

			if (_transaction != null && !_isTheirTransaction) {
				if (!_isCompleted)
					_transaction.Rollback();

				_transaction.Dispose();
			}

			_isDisposed = true;
		}

		public ISelect<T> Query<T>() {
			return _queryFactory.Select<T>(this);
		}

		public IEnumerable<T> Query<T>(ISelect<T> query) {
			return _engine.Query(Connection, query);
		}

		public ISelect<T> QueryTracked<T>() {
			throw new NotImplementedException();
		}

		public IEnumerable<T> Query<T>(Query<T> query) {
			throw new NotImplementedException();
		}

		public void Insert<T>(T entity) {
			throw new NotImplementedException();
		}

		public void Update<T>(T entity) {
			throw new NotImplementedException();
		}

		public IWhereExecute<T> Update<T>() {
			throw new NotImplementedException();
		}

		public void Delete<T>(T entity) {
			throw new NotImplementedException();
		}

		public void Delete<T>(int id) {
			throw new NotImplementedException();
		}

		public IWhereExecute<T> Delete<T>() {
			throw new NotImplementedException();
		}
	}
}