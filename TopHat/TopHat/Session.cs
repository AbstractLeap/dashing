using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using TopHat.Configuration;
using TopHat.SqlWriter;

namespace TopHat {
	public sealed class Session : ISession {
		private readonly ISqlWriter _writer;
		private readonly IQueryFactory _queryFactory;
		private readonly IDbConnection _connection;
		private readonly bool _isTheirTransaction;
		private IDbTransaction _transaction;
		private bool _isDisposed;
		private bool _isCompleted;

		public Session(ISqlWriter sqlWriter, IQueryFactory queryFactory, IDbConnection connection) {
			if (sqlWriter == null) throw new ArgumentNullException("writer");
			if (queryFactory == null) throw new ArgumentNullException("queryFactory");
			if (connection == null) throw new ArgumentNullException("connection");

			_writer = sqlWriter;
			_queryFactory = queryFactory;
			_connection = connection;
		}

		public Session(
			ISqlWriter sqlWriter,
			IQueryFactory queryFactory,
			IDbConnection connection,
			IDbTransaction transaction = null) {
			if (sqlWriter == null) throw new ArgumentNullException("writer");
			if (queryFactory == null) throw new ArgumentNullException("queryFactory");
			if (connection == null) throw new ArgumentNullException("connection");

			_writer = sqlWriter;
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

		public IEnumerable<T> Query<T>(Query<T> query) {
			var sqlQuery = _writer.WriteSqlFor(query);
			return Connection.Query<T>(sqlQuery.Sql, sqlQuery.Parameters);
		}

		public int Execute<T>(Query<T> query) {
			var sqlQuery = _writer.WriteSqlFor(query);
			return Connection.Execute(sqlQuery.Sql, sqlQuery.Parameters);
		}

		public ISelect<T> QueryTracked<T>() {
			throw new NotImplementedException();
			return new QueryWriter<T>(this, true);
		}
		public void Insert<T>(T entity) {
			throw new NotImplementedException();
			var query = new Query<T> { Entity = entity, QueryType = QueryType.Insert };
			Execute(query);
		}

		public void Update<T>(T entity) {
			throw new NotImplementedException();
			var query = new Query<T> { Entity = entity, QueryType = QueryType.Update };
			Execute(query);
		}

		public IWhereExecute<T> Update<T>() {
			throw new NotImplementedException();
			return new WhereExecuter<T>(this, QueryType.Update);
		}

		public void Delete<T>(T entity) {
			throw new NotImplementedException();
			var query = new Query<T> { Entity = entity, QueryType = QueryType.Delete };
			Execute(query);
		}

		public void Delete<T>(int id) {
			throw new NotImplementedException();
		}

		public IWhereExecute<T> Delete<T>() {
			throw new NotImplementedException();
			return new WhereExecuter<T>(this, QueryType.Delete);
		}
	}
}