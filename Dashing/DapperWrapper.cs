namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dapper;

    // ReSharper disable InvokeAsExtensionMethod
    public class DapperWrapper : IDapper {
        private readonly ISessionState sessionState;

        public DapperWrapper(ISessionState sessionState) {
            this.sessionState = sessionState;
        }

        public int Execute(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Execute(this.sessionState.GetConnection(), sql, param, this.sessionState.GetTransaction(), commandTimeout, commandType);
        }

        public int Execute(CommandDefinition command) {
            return SqlMapper.Execute(this.sessionState.GetConnection(), this.ReplaceTransactionIn(command));
        }

        public async Task<int> ExecuteAsync(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return
                await
                SqlMapper.ExecuteAsync(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    commandTimeout,
                    commandType);
        }

        public async Task<int> ExecuteAsync(CommandDefinition command) {
            return await SqlMapper.ExecuteAsync(await this.sessionState.GetConnectionAsync(), await this.ReplaceTransactionInAsync(command));
        }

        public IDataReader ExecuteReader(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.ExecuteReader(
                this.sessionState.GetConnection(),
                sql,
                param,
                this.sessionState.GetTransaction(),
                commandTimeout,
                commandType);
        }

        public IDataReader ExecuteReader(CommandDefinition command) {
            return SqlMapper.ExecuteReader(this.sessionState.GetConnection(), this.ReplaceTransactionIn(command));
        }

        public IEnumerable<dynamic> Query(
            string sql,
            dynamic param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query(
                this.sessionState.GetConnection(),
                sql,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                commandTimeout,
                commandType);
        }

        public IEnumerable<T> Query<T>(
            string sql,
            dynamic param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<T>(
                this.sessionState.GetConnection(),
                sql,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                commandTimeout,
                commandType);
        }

        public IEnumerable<T> Query<T>(CommandDefinition command) {
            return SqlMapper.Query<T>(this.sessionState.GetConnection(), this.ReplaceTransactionIn(command));
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TReturn>(
                this.sessionState.GetConnection(),
                sql,
                map,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TReturn>(
                this.sessionState.GetConnection(),
                sql,
                map,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TReturn>(
                this.sessionState.GetConnection(),
                sql,
                map,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
                this.sessionState.GetConnection(),
                sql,
                map,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
                this.sessionState.GetConnection(),
                sql,
                map,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
                this.sessionState.GetConnection(),
                sql,
                map,
                param,
                this.sessionState.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public async Task<IEnumerable<dynamic>> QueryAsync(
            string sql,
            dynamic param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            dynamic param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<T>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command) {
            return await SqlMapper.QueryAsync<T>(await this.sessionState.GetConnectionAsync(), await this.ReplaceTransactionInAsync(command));
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TReturn>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TReturn>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            dynamic param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
                    await this.sessionState.GetConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.sessionState.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.QueryMultiple(
                this.sessionState.GetConnection(),
                sql,
                param,
                this.sessionState.GetTransaction(),
                commandTimeout,
                commandType);
        }

        public SqlMapper.GridReader QueryMultiple(CommandDefinition command) {
            return SqlMapper.QueryMultiple(this.sessionState.GetConnection(), this.ReplaceTransactionIn(command));
        }

        private CommandDefinition ReplaceTransactionIn(CommandDefinition command) {
            return new CommandDefinition(
                command.CommandText,
                command.Parameters,
                this.sessionState.GetTransaction(),
                command.CommandTimeout,
                command.CommandType,
                command.Flags,
                command.CancellationToken);
        }

        private async Task<CommandDefinition> ReplaceTransactionInAsync(CommandDefinition command) {
            return new CommandDefinition(
                command.CommandText,
                command.Parameters,
                await this.sessionState.GetTransactionAsync(),
                command.CommandTimeout,
                command.CommandType,
                command.Flags,
                command.CancellationToken);
        }
    }

    // ReSharper restore InvokeAsExtensionMethod
}