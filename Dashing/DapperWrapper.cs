namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dapper;

    // ReSharper disable InvokeAsExtensionMethod
    public class DapperWrapper : IDapper {
        private readonly Lazy<IDbConnection> connection;

        private readonly Lazy<IDbTransaction> transaction;

        public DapperWrapper(Lazy<IDbConnection> connection, Lazy<IDbTransaction> transaction) {
            this.connection = connection;
            this.transaction = transaction;
        }

        public int Execute(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Execute(this.connection.Value, sql, param, this.transaction.Value, commandTimeout, commandType);
        }

        public int Execute(CommandDefinition command) {
            return SqlMapper.Execute(this.connection.Value, this.ReplaceTransactionIn(command));
        }

        public IDataReader ExecuteReader(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.ExecuteReader(this.connection.Value, sql, param, this.transaction.Value, commandTimeout, commandType);
        }

        public IDataReader ExecuteReader(CommandDefinition command) {
            return SqlMapper.ExecuteReader(this.connection.Value, this.ReplaceTransactionIn(command));
        }

        public IEnumerable<dynamic> Query(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query(this.connection.Value, sql, param, this.transaction.Value, buffered, commandTimeout, commandType);
        }

        public IEnumerable<T> Query<T>(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<T>(this.connection.Value, sql, param, this.transaction.Value, buffered, commandTimeout, commandType);
        }

        public IEnumerable<T> Query<T>(CommandDefinition command) {
            return SqlMapper.Query<T>(this.connection.Value, this.ReplaceTransactionIn(command));
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TReturn>(this.connection.Value, sql, map, param, this.transaction.Value, buffered, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TReturn>(this.connection.Value, sql, map, param, this.transaction.Value, buffered, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TReturn>(this.connection.Value, sql, map, param, this.transaction.Value, buffered, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this.connection.Value, sql, map, param, this.transaction.Value, buffered, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this.connection.Value, sql, map, param, this.transaction.Value, buffered, splitOn, commandTimeout, commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this.connection.Value, sql, map, param, this.transaction.Value, buffered, splitOn, commandTimeout, commandType);
        }

        public SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.QueryMultiple(this.connection.Value, sql, param, this.transaction.Value, commandTimeout, commandType);
        }

        public SqlMapper.GridReader QueryMultiple(CommandDefinition command) {
            return SqlMapper.QueryMultiple(this.connection.Value, this.ReplaceTransactionIn(command));
        }

        private CommandDefinition ReplaceTransactionIn(CommandDefinition command) {
            return new CommandDefinition(command.CommandText, command.Parameters, this.transaction.Value, command.CommandTimeout, command.CommandType, command.Flags, command.CancellationToken);
        }
    }
    // ReSharper restore InvokeAsExtensionMethod
}