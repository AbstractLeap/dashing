namespace Dashing.Testing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dapper;

    internal class MockDapper : IDapper {
        public MockDapper() {
            this.Commands = new List<CommandDefinition>();
        }

        public IList<CommandDefinition> Commands { get; set; }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            this.Commands.Add(new CommandDefinition(sql, param));
            return 1;
        }

        public int Execute(CommandDefinition command) {
            this.Commands.Add(command);
            return 1;
        }

        public Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return Task.FromResult(this.Execute(sql, param, commandTimeout, commandType));
        }

        public Task<int> ExecuteAsync(CommandDefinition command) {
            return Task.FromResult(this.Execute(command));
        }

        public IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IDataReader ExecuteReader(CommandDefinition command) {
            throw new NotImplementedException();
        }

        public IEnumerable<dynamic> Query(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>(CommandDefinition command) {
            throw new NotImplementedException();
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<dynamic>> QueryAsync(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public SqlMapper.GridReader QueryMultiple(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public SqlMapper.GridReader QueryMultiple(CommandDefinition command) {
            throw new NotImplementedException();
        }


        public IEnumerable<TReturn> Query<TReturn>(string sql, Type[] types, Func<object[], TReturn> map, object param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TReturn>(string sql, Type[] types, Func<object[], TReturn> map, object param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            throw new NotImplementedException();
        }
    }
}