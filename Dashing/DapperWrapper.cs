namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dapper;

    using Dashing.Extensions;

    // ReSharper disable InvokeAsExtensionMethod
    public class DapperWrapper : IDapper {
        private readonly Session session;

        public DapperWrapper(Session session) {
            this.session = session;
        }

        public int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.Execute(this.session.MaybeOpenConnection(), sql, param, this.session.GetTransaction(), commandTimeout, commandType);
        }

        public int Execute(CommandDefinition command) {
            return SqlMapper.Execute(this.session.MaybeOpenConnection(), this.ReplaceTransactionIn(command));
        }

        public async Task<int> ExecuteAsync(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return
                await
                SqlMapper.ExecuteAsync(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    param,
                    await this.session.GetTransactionAsync(),
                    commandTimeout,
                    commandType);
        }

        public async Task<int> ExecuteAsync(CommandDefinition command) {
            return await SqlMapper.ExecuteAsync(await this.session.MaybeOpenConnectionAsync(), await this.ReplaceTransactionInAsync(command));
        }

        public IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.ExecuteReader(
                this.session.MaybeOpenConnection(),
                sql,
                param,
                this.session.GetTransaction(),
                commandTimeout,
                commandType);
        }

        public IDataReader ExecuteReader(CommandDefinition command) {
            return SqlMapper.ExecuteReader(this.session.MaybeOpenConnection(), this.ReplaceTransactionIn(command));
        }

        public IEnumerable<dynamic> Query(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query(
                this.session.MaybeOpenConnection(),
                sql,
                param,
                this.session.GetTransaction(),
                buffered,
                commandTimeout,
                commandType);
        }

        public IEnumerable<T> Query<T>(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<T>(
                this.session.MaybeOpenConnection(),
                sql,
                param,
                this.session.GetTransaction(),
                buffered,
                commandTimeout,
                commandType);
        }

        public IEnumerable<T> Query<T>(CommandDefinition command) {
            return SqlMapper.Query<T>(this.session.MaybeOpenConnection(), this.ReplaceTransactionIn(command));
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TReturn>(
                this.session.MaybeOpenConnection(),
                sql,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TReturn>(
                this.session.MaybeOpenConnection(),
                sql,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TReturn>(
                this.session.MaybeOpenConnection(),
                sql,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
                this.session.MaybeOpenConnection(),
                sql,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
                this.session.MaybeOpenConnection(),
                sql,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
                this.session.MaybeOpenConnection(),
                sql,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        }

        public IEnumerable<TReturn> Query<TReturn>(string sql, 
            Type[] types,
            Func<object[], TReturn> map,
            object param = null, 
            bool buffered = true, 
            string splitOn = "Id", 
            int? commandTimeout = null, 
            CommandType? commandType = null) {
            return SqlMapper.Query(
                this.session.MaybeOpenConnection(),
                sql,
                types,
                map,
                param,
                this.session.GetTransaction(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        } 

        public async Task<IEnumerable<dynamic>> QueryAsync(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    param,
                    await this.session.GetTransactionAsync(),
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object param = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<T>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    param,
                    await this.session.GetTransactionAsync(),
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command) {
            return await SqlMapper.QueryAsync<T>(await this.session.MaybeOpenConnectionAsync(), await this.ReplaceTransactionInAsync(command));
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql,
            Func<TFirst, TSecond, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TReturn>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.session.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TReturn>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.session.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.session.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.session.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.session.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            string sql,
            Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return
                await
                SqlMapper.QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
                    await this.session.MaybeOpenConnectionAsync(),
                    sql,
                    map,
                    param,
                    await this.session.GetTransactionAsync(),
                    buffered,
                    splitOn,
                    commandTimeout,
                    commandType);
        }

        public async Task<IEnumerable<TReturn>> QueryAsync<TReturn>(string sql,
            Type[] types,
            Func<object[], TReturn> map,
            object param = null,
            bool buffered = true,
            string splitOn = "Id",
            int? commandTimeout = null,
            CommandType? commandType = null) {
            return SqlMapper.Query(
                await this.session.MaybeOpenConnectionAsync(),
                sql,
                types,
                map,
                param,
                await this.session.GetTransactionAsync(),
                buffered,
                splitOn,
                commandTimeout,
                commandType);
        } 

        public SqlMapper.GridReader QueryMultiple(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null) {
            return SqlMapper.QueryMultiple(
                this.session.MaybeOpenConnection(),
                sql,
                param,
                this.session.GetTransaction(),
                commandTimeout,
                commandType);
        }

        public SqlMapper.GridReader QueryMultiple(CommandDefinition command) {
            return SqlMapper.QueryMultiple(this.session.MaybeOpenConnection(), this.ReplaceTransactionIn(command));
        }

        private CommandDefinition ReplaceTransactionIn(CommandDefinition command) {
            return new CommandDefinition(
                command.CommandText,
                command.Parameters,
                this.session.GetTransaction(),
                command.CommandTimeout,
                command.CommandType,
                command.Flags,
                command.CancellationToken);
        }

        private async Task<CommandDefinition> ReplaceTransactionInAsync(CommandDefinition command) {
            return new CommandDefinition(
                command.CommandText,
                command.Parameters,
                await this.session.GetTransactionAsync(),
                command.CommandTimeout,
                command.CommandType,
                command.Flags,
                command.CancellationToken);
        }
    }

    // ReSharper restore InvokeAsExtensionMethod
}