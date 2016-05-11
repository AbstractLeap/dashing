namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dapper;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;

    public class SqlEngine : IEngine {
        private readonly ISqlDialect dialect;

        private IConfiguration configuration;

        private ISelectWriter selectWriter;

        private ICountWriter countWriter;

        private IUpdateWriter updateWriter;

        private IInsertWriter insertWriter;

        private IDeleteWriter deleteWriter;

        private DelegateQueryCreator delegateQueryCreator;

        private delegate IEnumerable<T> DelegateQuery<T>(
            SelectWriterResult result,
            SelectQuery<T> query,
            IDbConnection connection,
            IDbTransaction transaction) where T : class, new();

        private delegate Task<IEnumerable<T>> DelegateQueryAsync<T>(
            SelectWriterResult result,
            SelectQuery<T> query,
            IDbConnection connection,
            IDbTransaction transaction) where T : class, new();

        public ISqlDialect SqlDialect
        {
            get
            {
                return this.dialect;
            }
        }

        public IConfiguration Configuration
        {
            get
            {
                return this.configuration;
            }

            set
            {
                this.configuration = value;
                this.selectWriter = new SelectWriter(this.dialect, this.Configuration);
                this.countWriter = new CountWriter(this.dialect, this.Configuration);
                this.deleteWriter = new DeleteWriter(this.dialect, this.Configuration);
                this.updateWriter = new UpdateWriter(this.dialect, this.Configuration);
                this.insertWriter = new InsertWriter(this.dialect, this.Configuration);
            }
        }

        public SqlEngine(ISqlDialect dialect) {
            this.dialect = dialect;
        }

        public T Query<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id)where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var entity = sessionState.GetConnection().Query<T>(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction()).SingleOrDefault();
            if (entity != null) {
                ((ITrackedEntity)entity).EnableTracking();
            }

            return entity;
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return sessionState.GetConnection().Query<T>(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction()).Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
        }

        public virtual IEnumerable<T> Query<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateSql(query);
            if (query.HasFetches()) {
                Func<SelectWriterResult, SelectQuery<T>, IDbConnection, IDbTransaction, IEnumerable<T>> queryFunc;
                if (sqlQuery.NumberCollectionsFetched > 0) {
                    queryFunc = this.delegateQueryCreator.GetCollectionFunction<T>(sqlQuery);
                }
                else {
                    queryFunc = this.delegateQueryCreator.GetNoCollectionFunction<T>(sqlQuery);
                }

                return queryFunc(sqlQuery, query, sessionState.GetConnection(), sessionState.GetTransaction());
            }

            return sessionState.GetConnection().Query<T>(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction()).Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
            ;
        }

        public Page<T> QueryPaged<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults =
                sessionState.GetConnection().Query<int>(countQuery.Sql, countQuery.Parameters, sessionState.GetTransaction()).SingleOrDefault();

            return new Page<T> {
                                   TotalResults = totalResults,
                                   Items = this.Query(sessionState, query).ToArray(),
                                   Skipped = query.SkipN,
                                   Taken = query.TakeN
                               };
        }

        public int Count<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            return sessionState.GetConnection().Query<int>(countQuery.Sql, countQuery.Parameters, sessionState.GetTransaction()).SingleOrDefault();
        }

        public virtual int Insert<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.EnsureConfigurationLoaded();

            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    if (map.PrimaryKey.Type == typeof(Int32)) {
                        var idResult = sessionState.GetConnection()
                                                   .Query<int>(
                                                       sqlQuery.Sql + ";" + getLastInsertedId,
                                                       sqlQuery.Parameters,
                                                       sessionState.GetTransaction());
                        map.SetPrimaryKeyValue(entity, idResult.Single());
                    }
                    else if (map.PrimaryKey.Type == typeof(Int64)) {
                        var idResult = sessionState.GetConnection()
                                                   .Query<long>(
                                                       sqlQuery.Sql + ";" + getLastInsertedId,
                                                       sqlQuery.Parameters,
                                                       sessionState.GetTransaction());
                        map.SetPrimaryKeyValue(entity, idResult.Single());
                    }
                    else {
                        throw new NotSupportedException("Auto generated primary keys with types other than Int32 and Int64 are not supported");
                    }
                }
                else {
                    sessionState.GetConnection().Execute(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction());
                }

                ((ITrackedEntity)entity).EnableTracking(); // turn on tracking
                ++i;
            }

            return i;
        }

        public virtual int Save<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0
                       ? 0
                       : sessionState.GetConnection().Execute(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction());
        }

        public virtual int Delete<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return 0;
            }

            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return sessionState.GetConnection().Execute(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction());
        }

        public int Execute<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateBulkSql(update, predicates);
            return sqlQuery.Sql.Length == 0
                       ? 0
                       : sessionState.GetConnection().Execute(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction());
        }

        public int ExecuteBulkDelete<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateBulkSql(predicates);
            return sessionState.GetConnection().Execute(sqlQuery.Sql, sqlQuery.Parameters, sessionState.GetTransaction());
        }

        public async Task<T> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var queryResult =
                await
                (await sessionState.GetConnectionAsync()).QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, await sessionState.GetTransactionAsync());
            var entity = queryResult.SingleOrDefault();
            if (entity != null) {
                ((ITrackedEntity)entity).EnableTracking();
            }

            return entity;
        }

        public async Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            var result =
                await
                (await sessionState.GetConnectionAsync()).QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, await sessionState.GetTransactionAsync());
            return result.Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateSql(query);
            IEnumerable<T> queryResults;
            if (query.HasFetches()) {
                if (sqlQuery.NumberCollectionsFetched > 0) {
                    queryResults =
                        await
                        this.delegateQueryCreator.GetAsyncCollectionFunction<T>(sqlQuery)(
                            sqlQuery,
                            query,
                            await sessionState.GetConnectionAsync(),
                            await sessionState.GetTransactionAsync());
                }
                else {
                    queryResults =
                        await
                        this.delegateQueryCreator.GetAsyncNoCollectionFunction<T>(sqlQuery)(
                            sqlQuery,
                            query,
                            await sessionState.GetConnectionAsync(),
                            await sessionState.GetTransactionAsync());
                }
            }
            else {
                queryResults =
                    (await
                     (await sessionState.GetConnectionAsync()).QueryAsync<T>(
                         sqlQuery.Sql,
                         sqlQuery.Parameters,
                         await sessionState.GetTransactionAsync())).Select(
                             t => {
                                 ((ITrackedEntity)t).EnableTracking();
                                 return t;
                             });
                ;
            }

            return queryResults;
        }

        public async Task<Page<T>> QueryPagedAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults =
                (await
                 (await sessionState.GetConnectionAsync()).QueryAsync<int>(
                     countQuery.Sql,
                     countQuery.Parameters,
                     await sessionState.GetTransactionAsync())).SingleOrDefault();
            var results = await this.QueryAsync(sessionState, query);

            return new Page<T> { TotalResults = totalResults, Items = results.ToArray(), Skipped = query.SkipN, Taken = query.TakeN };
        }

        public async Task<int> CountAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            return
                (await
                 (await sessionState.GetConnectionAsync()).QueryAsync<int>(
                     countQuery.Sql,
                     countQuery.Parameters,
                     await sessionState.GetTransactionAsync())).SingleOrDefault();
        }

        public async Task<int> InsertAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.EnsureConfigurationLoaded();

            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    var sqlAndReturnId = sqlQuery.Sql + ";" + getLastInsertedId;
                    if (map.PrimaryKey.Type == typeof(int)) {
                        var idResult =
                            await
                            (await sessionState.GetConnectionAsync()).QueryAsync<int>(
                                sqlAndReturnId,
                                sqlQuery.Parameters,
                                await sessionState.GetTransactionAsync());
                        map.SetPrimaryKeyValue(entity, idResult.Single());
                    }
                    else if (map.PrimaryKey.Type == typeof(Int64)) {
                        var idResult =
                            await
                            (await sessionState.GetConnectionAsync()).QueryAsync<long>(
                                sqlAndReturnId,
                                sqlQuery.Parameters,
                                await sessionState.GetTransactionAsync());
                        map.SetPrimaryKeyValue(entity, idResult.Single());
                    }
                    else {
                        throw new NotSupportedException("Auto generated primary keys with types other than Int32 and Int64 are not supported");
                    }
                }
                else {
                    await
                        (await sessionState.GetConnectionAsync()).ExecuteAsync(
                            sqlQuery.Sql,
                            sqlQuery.Parameters,
                            await sessionState.GetTransactionAsync());
                }

                ((ITrackedEntity)entity).EnableTracking(); // turn on tracking
                ++i;
            }

            return i;
        }

        public async Task<int> SaveAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0
                       ? 0
                       : await
                         (await sessionState.GetConnectionAsync()).ExecuteAsync(
                             sqlQuery.Sql,
                             sqlQuery.Parameters,
                             await sessionState.GetTransactionAsync());
        }

        public async Task<int> DeleteAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return 0;
            }

            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return
                await
                (await sessionState.GetConnectionAsync()).ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, await sessionState.GetTransactionAsync());
        }

        public async Task<int> ExecuteAsync<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateBulkSql(update, predicates);
            return sqlQuery.Sql.Length == 0
                       ? 0
                       : await
                         (await sessionState.GetConnectionAsync()).ExecuteAsync(
                             sqlQuery.Sql,
                             sqlQuery.Parameters,
                             await sessionState.GetTransactionAsync());
        }

        public async Task<int> ExecuteBulkDeleteAsync<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateBulkSql(predicates);
            return
                await
                (await sessionState.GetConnectionAsync()).ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, await sessionState.GetTransactionAsync());
        }

        private void EnsureConfigurationLoaded() {
            if (this.configuration == null) {
                throw new InvalidOperationException("Configuration was not injected into the Engine properly");
            }

            if (this.delegateQueryCreator == null) {
                this.delegateQueryCreator = new DelegateQueryCreator(this.configuration);
            }
        }
    }
}