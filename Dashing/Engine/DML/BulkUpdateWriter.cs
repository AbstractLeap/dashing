namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    internal class BulkUpdateWriter : BaseWriter, IBulkUpdateWriter {
        public BulkUpdateWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) { }

        public SqlWriterResult GenerateBulkSql<T>(Action<T> updateAction, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            var predicateArray = predicates as Expression<Func<T, bool>>[] ?? predicates?.ToArray();

            // add where clause
            var whereSql = new StringBuilder();
            var parameters = new AutoNamingDynamicParameters();
            var aliasBucket = new DefaultAliasProvider();
            QueryTree queryTree = null;
            if (predicateArray != null) {
                this.AddWhereClause(predicateArray, whereSql, parameters, aliasBucket, ref queryTree);
            }

            // run the update
            var entity = new T();
            ((ISetLogger)entity).EnableSetLogging();
            updateAction(entity);

            // find the set properties
            var setLogger = (ISetLogger)entity;
            var setProps = setLogger.GetSetProperties();
            if (!setProps.Any()) {
                return new SqlWriterResult(string.Empty, parameters);
            }

            if (queryTree == null) {
                // the where clauses on are on the root table
                return new SqlWriterResult(this.GetSimpleUpdateQuery(setProps, entity, parameters, whereSql), parameters);
            }

            // cross table where clause
            return new SqlWriterResult(this.GetMultiTableUpdateQuery(setProps, entity, parameters, aliasBucket, whereSql, queryTree), parameters);
        }

        private string GetMultiTableUpdateQuery<T>(IEnumerable<string> setProps, T entity, AutoNamingDynamicParameters parameters, IAliasProvider aliasProvider, StringBuilder whereSql, QueryTree rootQueryNode) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();

            sql.Append("update t set ");

            this.AddSetClauses(setProps, map, sql, entity, parameters, true);

            sql.Append(" from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" as t");

            foreach (var node in rootQueryNode.Children) {
                this.AddNode(node.Value, sql, aliasProvider);
            }

            sql.Append(whereSql);
            return sql.ToString();
        }

        private string GetSimpleUpdateQuery<T>(IEnumerable<string> setProps, T entity, AutoNamingDynamicParameters parameters, StringBuilder whereSql) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();

            sql.Append("update ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" set ");

            this.AddSetClauses(setProps, map, sql, entity, parameters, false);
            sql.Append(whereSql);
            return sql.ToString();
        }

        private void AddSetClauses<T>(IEnumerable<string> setProps, IMap<T> map, StringBuilder sql, T entity, AutoNamingDynamicParameters parameters, bool includeRootAlias) {
            foreach (var updatedPropEntry in setProps.AsSmartEnumerable()) {
                if (includeRootAlias) {
                    sql.Append("t.");
                }

                var column = map.Columns[updatedPropEntry.Value];
                this.Dialect.AppendQuotedName(sql, column.DbName);
                var paramName = "@" + updatedPropEntry.Value;
                var propertyValue = map.GetColumnValue(entity, column);
                if (propertyValue == null) {
                    parameters.Add(paramName, null);
                }
                else {
                    parameters.Add(paramName, this.GetValueOrPrimaryKey(column, propertyValue));
                }

                sql.Append(" = ");
                sql.Append(paramName);
                if (!updatedPropEntry.IsLast) {
                    sql.Append(", ");
                }
            }
        }
    }
}