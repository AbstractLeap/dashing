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
        private IConfiguration configuration;

        private ISelectWriter selectWriter;

        private ICountWriter countWriter;

        private IUpdateWriter updateWriter;

        private IBulkUpdateWriter bulkUpdateWriter;

        private IInsertWriter insertWriter;

        private IDeleteWriter deleteWriter;

        private IBulkDeleteWriter bulkDeleteWriter;

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

        public ISqlDialect SqlDialect { get; }

        public IConfiguration Configuration {
            get {
                return this.configuration;
            }
        }

        public SqlEngine(IConfiguration configuration, ISqlDialect dialect) {
            this.SqlDialect = dialect;
            this.configuration = configuration;
            this.selectWriter = new SelectWriter(dialect, configuration);
            this.countWriter = new CountWriter(dialect, configuration);
            this.updateWriter = new UpdateWriter(dialect, configuration);
            this.insertWriter = new InsertWriter(dialect, configuration);
            this.deleteWriter = new DeleteWriter(dialect, configuration);
            this.bulkUpdateWriter = new BulkUpdateWriter(dialect, configuration);
            this.bulkDeleteWriter = new BulkDeleteWriter(dialect, configuration);
            this.delegateQueryCreator = new DelegateQueryCreator(configuration);
        }

        public T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new() {
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var entity = connection.Query<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction).SingleOrDefault();
            if (entity != null) {
                ((ITrackedEntity)entity).EnableTracking();
            }

            return entity;
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids)
            where T : class, new() {
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return connection.Query<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction).Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
        }

        public virtual IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
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
                });
            ;
        }

        public Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = connection.Query<int>(countQuery.Sql, countQuery.Parameters, transaction).SingleOrDefault();

            return new Page<T> {
                                   TotalResults = totalResults,
                                   Items = this.Query(connection, transaction, query).ToArray(),
                                   Skipped = query.SkipN,
                                   Taken = query.TakeN
                               };
        }

        public IEnumerable<TProjection> Query<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var sqlResult = this.selectWriter.GenerateSql(query);
            if (sqlResult.MapQueryTree.Children.Count == 0) {
                return connection.Query<TBase>(sqlResult.Sql, sqlResult.Parameters, transaction)
                                 .Select(query.ProjectionExpression.Compile());
            }

            var projectionExpressionRewriter = new ProjectionExpressionRewriter<TBase, TProjection>(this.configuration, query, sqlResult.MapQueryTree);
            var projectionDelegateResult = projectionExpressionRewriter.Rewrite();
            return connection.Query<TProjection>(sqlResult.Sql, projectionDelegateResult.Types, projectionDelegateResult.Mapper, sqlResult.Parameters, transaction, splitOn: sqlResult.MapQueryTree.GetSplitOn());
        }

        public Page<TProjection> QueryPaged<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = connection.Query<int>(countQuery.Sql, countQuery.Parameters, transaction).SingleOrDefault();
            return new Page<TProjection> {
                                             TotalResults = totalResults,
                                             Items = this.Query(connection, transaction, query).ToArray(),
                                             Skipped = query.BaseSelectQuery.SkipN,
                                             Taken = query.BaseSelectQuery.TakeN
                                         };
        }

        public int Count<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            var countQuery = this.countWriter.GenerateCountSql(query);
            return connection.Query<int>(countQuery.Sql, countQuery.Parameters, transaction).SingleOrDefault();
        }

        public virtual int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    var sqlAndReturnId = sqlQuery.Sql + ";" + getLastInsertedId;
                    if (map.PrimaryKey.Type == typeof(Int32)) {
                        InsertAndSetId<T, int>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters, map, entity);
                    }
                    else if (map.PrimaryKey.Type == typeof(Int64)) {
                        InsertAndSetId<T, long>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters, map, entity);
                    }
                    else if (map.PrimaryKey.Type == typeof(Guid)) {
                        InsertAndSetId<T, Guid>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters, map, entity);
                    }
                    else {
                        throw new NotSupportedException("Auto generated primary keys with types other than Int32, Int64 and Guid are not supported");
                    }
                }
                else {
                    connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
                }

                ((ITrackedEntity)entity).EnableTracking(); // turn on tracking
                ++i;
            }

            return i;
        }

        private static void InsertAndSetId<T, TKey>(
            IDbConnection connection,
            IDbTransaction transaction,
            string sqlAndReturnId,
            DynamicParameters parameters,
            IMap<T> map,
            T entity) where T : class, new()
        {
            var idResult = connection
                                       .Query<TKey>(
                                           sqlAndReturnId,
                                           parameters,
                                           transaction);
            map.SetPrimaryKeyValue(entity, idResult.Single());
        }

        public virtual int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0 ? 0 : connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public virtual int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return 0;
            }
            
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public int Execute<T>(
            IDbConnection connection,
            IDbTransaction transaction,
            Action<T> update,
            IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            var sqlQuery = this.bulkUpdateWriter.GenerateBulkSql(update, predicates);
            return sqlQuery.Sql.Length == 0 ? 0 : connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            var sqlQuery = this.bulkDeleteWriter.GenerateBulkSql(predicates);
            return connection.Execute(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public async Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new() {
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var queryResult = await connection.QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction);
            var entity = queryResult.SingleOrDefault();
            if (entity != null) {
                ((ITrackedEntity)entity).EnableTracking();
            }

            return entity;
        }

        public async Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(
            IDbConnection connection,
            IDbTransaction transaction,
            IEnumerable<TPrimaryKey> ids) where T : class, new() {
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            var result = await connection.QueryAsync<T>(sqlQuery.Sql, sqlQuery.Parameters, transaction);
            return result.Select(
                t => {
                    ((ITrackedEntity)t).EnableTracking();
                    return t;
                });
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query)
            where T : class, new() {
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
                    });
                ;
            }

            return queryResults;
        }

        public async Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query)
            where T : class, new() {
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = (await connection.QueryAsync<int>(countQuery.Sql, countQuery.Parameters, transaction)).SingleOrDefault();
            var results = await this.QueryAsync(connection, transaction, query);

            return new Page<T> { TotalResults = totalResults, Items = results.ToArray(), Skipped = query.SkipN, Taken = query.TakeN };
        }

        public async Task<IEnumerable<TProjection>> QueryAsync<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var sqlResult = this.selectWriter.GenerateSql(query);
            if (sqlResult.MapQueryTree.Children.Count == 0) {
                var results = await connection.QueryAsync<TBase>(sqlResult.Sql, sqlResult.Parameters, transaction);
                return results.Select(query.ProjectionExpression.Compile());
            }

            var projectionExpressionRewriter = new ProjectionExpressionRewriter<TBase, TProjection>(this.configuration, query, sqlResult.MapQueryTree);
            var projectionDelegateResult = projectionExpressionRewriter.Rewrite();
            return await connection.QueryAsync<TProjection>(sqlResult.Sql, projectionDelegateResult.Types, projectionDelegateResult.Mapper, sqlResult.Parameters, transaction, splitOn: sqlResult.MapQueryTree.GetSplitOn());
        }

        public async Task<Page<TProjection>> QueryPagedAsync<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = (await connection.QueryAsync<int>(countQuery.Sql, countQuery.Parameters, transaction)).SingleOrDefault();
            return new Page<TProjection>
                   {
                       TotalResults = totalResults,
                       Items = (await this.QueryAsync(connection, transaction, query)).ToArray(),
                       Skipped = query.BaseSelectQuery.SkipN,
                       Taken = query.BaseSelectQuery.TakeN
                   };
        }

        public async Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            var countQuery = this.countWriter.GenerateCountSql(query);
            return (await connection.QueryAsync<int>(countQuery.Sql, countQuery.Parameters, transaction)).SingleOrDefault();
        }

        public async Task<int> InsertAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    var sqlAndReturnId = sqlQuery.Sql + ";" + getLastInsertedId;
                    if (map.PrimaryKey.Type == typeof(int)) {
                        await InsertAndSetIdAsync<T, int>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters, map, entity);
                    }
                    else if (map.PrimaryKey.Type == typeof(Int64)) {
                        await InsertAndSetIdAsync<T, long>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters, map, entity);
                    }
                    else if (map.PrimaryKey.Type == typeof(Guid)) {
                        await InsertAndSetIdAsync<T, Guid>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters, map, entity);
                    }
                    else {
                        throw new NotSupportedException("Auto generated primary keys with types other than Int32, Int64 and Guid are not supported");
                    }
                }
                else {
                    await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
                }

                ((ITrackedEntity)entity).EnableTracking(); // turn on tracking
                ++i;
            }

            return i;
        }

        private static async Task InsertAndSetIdAsync<T, TKey>(
            IDbConnection connection,
            IDbTransaction transaction,
            string sqlAndReturnId,
            DynamicParameters parameters,
            IMap<T> map,
            T entity) where T : class, new() {
            var idResult = await connection.QueryAsync<TKey>(
                               sqlAndReturnId,
                               parameters,
                               transaction);
            map.SetPrimaryKeyValue(entity, idResult.Single());
        }

        public async Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0 ? 0 : await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public async Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return 0;
            }
            
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public async Task<int> ExecuteAsync<T>(
            IDbConnection connection,
            IDbTransaction transaction,
            Action<T> update,
            IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            var sqlQuery = this.bulkUpdateWriter.GenerateBulkSql(update, predicates);
            return sqlQuery.Sql.Length == 0 ? 0 : await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public async Task<int> ExecuteBulkDeleteAsync<T>(
            IDbConnection connection,
            IDbTransaction transaction,
            IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            var sqlQuery = this.bulkDeleteWriter.GenerateBulkSql(predicates);
            return await connection.ExecuteAsync(sqlQuery.Sql, sqlQuery.Parameters, transaction);
        }

        public IDapper CreateDapperWrapper(ISession session) {
            if (session is Session theSession) {
                return new DapperWrapper(theSession);
            }

            throw new ArgumentOutOfRangeException(nameof(session), "SqlEngine expects ISession to be implemented by Session");
        }
    }
}