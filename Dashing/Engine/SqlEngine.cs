namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

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

        public T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id, bool isTracked) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            return this.configuration.CodeManager.Query<T>(sqlQuery, connection, transaction, isTracked).SingleOrDefault();
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids, bool isTracked) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, transaction, isTracked);
        }

        public virtual IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Query(sqlQuery, query, connection, transaction);
        }

        public Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = this.Configuration.CodeManager.QueryScalar<int>(countQuery.Sql, connection, transaction, countQuery.Parameters);

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
            return this.Configuration.CodeManager.QueryScalar<int>(countQuery.Sql, connection, transaction, countQuery.Parameters);
        }

        public virtual int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();

            var i = 0;
            var map = this.Configuration.GetMap<T>();
            var getLastInsertedId = this.insertWriter.GenerateGetIdSql<T>();

            foreach (var entity in entities) {
                var sqlQuery = this.insertWriter.GenerateSql(entity);
                if (map.PrimaryKey.IsAutoGenerated) {
                    var idResult = this.Configuration.CodeManager.Query<int>(
                        connection,
                        transaction,
                        sqlQuery.Sql + ";" + getLastInsertedId,
                        sqlQuery.Parameters);
                    map.SetPrimaryKeyValue(entity, idResult.Single());
                }
                else {
                    this.Configuration.CodeManager.ExecuteAsync(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
                }

                ++i;
            }

            return i;
        }

        public virtual int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0 ? 0 : this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public virtual int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return 0;
            }

            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.EnsureConfigurationLoaded();

            // generate a tracking class, apply the update, read out the updates
            var updateClass = this.Configuration.CodeManager.CreateUpdateInstance<T>();
            update(updateClass);
            var sqlQuery = this.updateWriter.GenerateBulkSql(updateClass, predicates);

            return sqlQuery.Sql.Length == 0 ? 0 : this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateBulkSql(predicates);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public async Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id, bool isTracked) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(id);
            var results = await this.configuration.CodeManager.QueryAsync<T>(sqlQuery, connection, transaction, isTracked);
            return results.SingleOrDefault();
        }

        public async Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids, bool isTracked) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return await this.Configuration.CodeManager.QueryAsync<T>(sqlQuery, connection, transaction, isTracked);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.selectWriter.GenerateSql(query);
            return this.Configuration.CodeManager.QueryAsync(sqlQuery, query, connection, transaction);
        }

        public async Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            var totalResults = await this.Configuration.CodeManager.QueryScalarAsync<int>(countQuery.Sql, connection, transaction, countQuery.Parameters);
            var results = await this.QueryAsync(connection, transaction, query);

            return new Page<T> { TotalResults = totalResults, Items = results.ToArray(), Skipped = query.SkipN, Taken = query.TakeN };
        }

        public Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) {
            this.EnsureConfigurationLoaded();
            var countQuery = this.countWriter.GenerateCountSql(query);
            return this.Configuration.CodeManager.QueryScalarAsync<int>(countQuery.Sql, connection, transaction, countQuery.Parameters);
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
                    var idResult = await this.Configuration.CodeManager.QueryAsync<int>(connection, transaction, sqlAndReturnId, sqlQuery.Parameters);
                    map.SetPrimaryKeyValue(entity, idResult.Single());
                }
                else {
                    await this.Configuration.CodeManager.ExecuteAsync(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
                }

                ++i;
            }

            return i;
        }

        public async Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.updateWriter.GenerateSql(entities);
            return sqlQuery.Sql.Length == 0 ? 0 : await this.Configuration.CodeManager.ExecuteAsync(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) {
            var entityArray = entities as T[] ?? entities.ToArray();

            // take the short path
            if (!entityArray.Any()) {
                return Task.FromResult<int>(0);
            }

            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateSql(entityArray);
            return this.Configuration.CodeManager.ExecuteAsync(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public Task<int> ExecuteAsync<T>(
            IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.EnsureConfigurationLoaded();

            // generate a tracking class, apply the update, read out the updates
            var updateClass = this.Configuration.CodeManager.CreateUpdateInstance<T>();
            update(updateClass);
            var sqlQuery = this.updateWriter.GenerateBulkSql(updateClass, predicates);

            return sqlQuery.Sql.Length == 0 ? Task.FromResult<int>(0) : this.Configuration.CodeManager.ExecuteAsync(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        public Task<int> ExecuteBulkDeleteAsync<T>(
            IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) {
            this.EnsureConfigurationLoaded();
            var sqlQuery = this.deleteWriter.GenerateBulkSql(predicates);
            return this.Configuration.CodeManager.ExecuteAsync(sqlQuery.Sql, connection, transaction, sqlQuery.Parameters);
        }

        private void EnsureConfigurationLoaded() {
            if (this.configuration == null) {
                throw new InvalidOperationException("Configuration was not injected into the Engine properly");
            }
        }
    }
}