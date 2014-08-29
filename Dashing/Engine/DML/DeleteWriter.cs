namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class DeleteWriter : BaseWriter, IDeleteWriter {
        public DeleteWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, new WhereClauseWriter(dialect, config), config) {
        }

        public DeleteWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) {
        }

        public SqlWriterResult GenerateSql<T>(IEnumerable<T> entities) {
            var entitiesArray = entities as T[] ?? entities.ToArray();

            if (!entitiesArray.Any()) {
                throw new ArgumentOutOfRangeException("entities", "Entities does not contain any members");
            }

            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();
            var paramIdx = 0;
            var parameters = new DynamicParameters();

            sql.Append("delete from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" where ");
            this.Dialect.AppendQuotedName(sql, map.PrimaryKey.DbName);
            sql.Append(" in (");

            foreach (var entity in entitiesArray) {
                var paramName = "@p_" + ++paramIdx;
                sql.Append(paramName);
                sql.Append(", ");
                parameters.Add(paramName, map.GetPrimaryKeyValue(entity));
            }

            sql.Remove(sql.Length - 2, 2); // remove trailing ,
            sql.Append(")");

            return new SqlWriterResult(sql.ToString(), parameters);
        }

        public SqlWriterResult GenerateBulkSql<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();

            sql.Append("delete from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            if (predicates != null) {
                this.AppendPredicates(predicates, sql, parameters);
            }

            return new SqlWriterResult(sql.ToString(), parameters);
        }

        private void AppendPredicates<T>(IEnumerable<Expression<Func<T, bool>>> predicates, StringBuilder sql, DynamicParameters parameters) {
            var predicateArray = predicates as Expression<Func<T, bool>>[] ?? predicates.ToArray();

            if (!predicateArray.Any()) {
                return;
            }

            var whereResult = this.WhereClauseWriter.GenerateSql(predicateArray, null);
            if (whereResult.FetchTree != null && whereResult.FetchTree.Children.Any()) {
                throw new NotImplementedException("Dashing does not currently support where clause across tables in a delete");
            }

            sql.Append(whereResult.Sql);
            parameters.AddDynamicParams(whereResult.Parameters);
        }
    }
}