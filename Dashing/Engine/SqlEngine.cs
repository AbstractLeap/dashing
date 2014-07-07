namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class SqlEngine : IEngine {
        public IConfiguration Configuration { get; set; }

        private ISelectWriter SelectWriter { get; set; }

        private IUpdateWriter UpdateWriter { get; set; }

        private IInsertWriter InsertWriter { get; set; }

        private IDeleteWriter DeleteWriter { get; set; }

        protected ISqlDialect Dialect { get; set; }

        public SqlEngine(ISqlDialect dialect) {
            this.Dialect = dialect;
            this.SelectWriter = new SelectWriter(this.Dialect, this.Configuration);
            this.DeleteWriter = new DeleteWriter(this.Dialect, this.Configuration);
            this.UpdateWriter = new UpdateWriter(this.Dialect, this.Configuration);
            this.InsertWriter = new InsertWriter(this.Dialect, this.Configuration);
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IEnumerable<TPrimaryKey> ids) {
            var sqlQuery = this.SelectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, this.Configuration.GetIsTrackedByDefault);
        }

        public IEnumerable<T> QueryTracked<T, TPrimaryKey>(IDbConnection connection, IEnumerable<TPrimaryKey> ids) {
            var sqlQuery = this.SelectWriter.GenerateGetSql<T, TPrimaryKey>(ids);
            return this.Configuration.CodeManager.Query<T>(sqlQuery, connection, true);
        }

        public virtual IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query) {
            var sqlQuery = this.SelectWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Query(sqlQuery, query, connection);
        }

        public virtual int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query) {
            foreach (var entity in query.Entities) {
                var sqlQuery = this.InsertWriter.GenerateSql(entity);
                this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);

                var map = this.Configuration.GetMap<T>();
                if (map.PrimaryKey.IsAutoGenerated) {
                    var idQuery = this.InsertWriter.GenerateGetIdSql<T>();
                    var id = this.Configuration.CodeManager.Query<int>(connection, idQuery).First();
                    this.Configuration.GetMap<T>().SetPrimaryKeyValue(entity, id);
                }
            }

            return query.Entities.Count;
        }

        public virtual int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query) {
            var sqlQuery = this.UpdateWriter.GenerateSql(query);
            return sqlQuery.Sql.Length == 0 ? 0 : this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }

        public virtual int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query) {
            var sqlQuery = this.DeleteWriter.GenerateSql(query);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }

        public int Execute<T>(IDbConnection connection, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) {
            // generate a tracking class, apply the update, read out the updates
            var updateClass = this.Configuration.CodeManager.CreateUpdateInstance<T>();
            update(updateClass);
            var sqlQuery = this.UpdateWriter.GenerateBulkSql(updateClass, predicates);

            return sqlQuery.Sql.Length == 0 ? 0 : this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }

        public int ExecuteBulkDelete<T>(IDbConnection connection, IEnumerable<Expression<Func<T, bool>>> predicates) {
            var sqlQuery = this.DeleteWriter.GenerateBulkSql(predicates);
            return this.Configuration.CodeManager.Execute(sqlQuery.Sql, connection, sqlQuery.Parameters);
        }
    }
}