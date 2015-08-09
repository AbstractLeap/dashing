namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.InteropServices;
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
            IDbTransaction transaction);

        private delegate Task<IEnumerable<T>> DelegateQueryAsync<T>(
            SelectWriterResult result,
            SelectQuery<T> query,
            IDbConnection connection,
            IDbTransaction transaction);

        public ISqlDialect SqlDialect {
            get {
                return this.dialect;
            }
        }

        public IConfiguration Configuration {
            get {
                return this.configuration;
            }

            set {
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

        public T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var entity = connection.Query<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction).SingleOrDefault();
            if (entity != null) {
                ((ITrackedEntity)entity).EnableTracking();
            }

            return entity;
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return connection.Query<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction).Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
        }

        public virtual IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
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

                return queryFunc(sqlQuery, query, connection, transaction);
            }

            return connection.Query<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction).Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                }); ;
        }

        public Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = connection.Query<int>(countQuery.Sql, countQuery.Parameters, transaction).SingleOrDefault();

            return new Page<T> {
                TotalResults = totalResults,
                Items = this.Query(connection, transaction, query).ToArray(),
                Skipped = query.SkipN,
                Taken = query.TakeN
            };
        }

        public int Count<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            return connection.Query<int>(countQuery.Sql, countQuery.Parameters, transaction).SingleOrDefault();
        }

        public virtual int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();

            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    var idResult = connection.Query<int>(
                        sqlQuery.Sql + ";" + getLastInsertedId,
                        sqlQuery.Parameters,
                        transaction);
                    map.SetPrimaryKeyValue(entity, idResult.Single());
                }
                else {
                    connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
                }

                ((ITrackedEntity)entity).EnableTracking(); // turn on tracking
                ++i;
            }

            return i;
        }

        public virtual int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0 ? 0 : connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public virtual int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return 0;
            }

            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.EnsureConfigurationLoaded();
           var sqlQuery = this.updateWriter.GenerateBulkSql(update, predicates);
           return sqlQuery.Sql.Length == 0 ? 0 : connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateBulkSql(predicates);
            return connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public async Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var queryResult = await connection.QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction);
            var entity = queryResult.SingleOrDefault();
            if (entity != null) {
                ((ITrackedEntity)entity).EnableTracking();
            }

            return entity;
        }

        public async Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            var result = await connection.QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction);
            return result.Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateSql(query);
            IEnumerable<T> queryResults;
            if (query.HasFetches()) {
                if (sqlQuery.NumberCollectionsFetched > 0) {
                    queryResults = await this.delegateQueryCreator.GetAsyncCollectionFunction<T>(sqlQuery)(sqlQuery, query, connection, transaction);
                }
                else {
                    queryResults = await this.delegateQueryCreator.GetAsyncNoCollectionFunction<T>(sqlQuery)(sqlQuery, query, connection, transaction);
                }
            }
            else {
                queryResults = (await connection.QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction)).Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                }); ;
            }

            return queryResults;
        }

        public async Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = (await connection.QueryAsync<int>(countQuery.Sql, countQuery.Parameters, transaction)).SingleOrDefault();
            var results = await this.QueryAsync(connection, transaction, query);

            return new Page<T> { TotalResults = totalResults, Items = results.ToArray(), Skipped = query.SkipN, Taken = query.TakeN };
        }

        public async Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            return (await connection.QueryAsync<int>(countQuery.Sql, countQuery.Parameters, transaction)).SingleOrDefault();
        }

        public async Task<int> InsertAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();

            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    var sqlAndReturnId = sqlQuery.Sql + ";" + getLastInsertedId;
                    var idResult = await connection.QueryAsync<int>(sqlAndReturnId, sqlQuery.Parameters, transaction);
                    map.SetPrimaryKeyValue(entity, idResult.Single());
                }
                else {
                    await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
                }

                ((ITrackedEntity)entity).EnableTracking(); // turn on tracking
                ++i;
            }

            return i;
        }

        public async Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0 ? 0 : await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters,transaction);
        }

        public Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return Task.FromResult<int>(0);
            }

            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public Task<int> ExecuteAsync<T>(
            IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateBulkSql(update, predicates);
            return sqlQuery.Sql.Length == 0 ? Task.FromResult<int>(0) : connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public Task<int> ExecuteBulkDeleteAsync<T>(
            IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateBulkSql(predicates);
            return connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
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