namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class BulkDeleteWriter : BaseWriter, IBulkDeleteWriter {
        public BulkDeleteWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) { }

        public SqlWriterResult GenerateBulkSql<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            var predicateArray = predicates as Expression<Func<T, bool>>[] ?? predicates?.ToArray();

            // add where clause
            var whereSql = new StringBuilder();
            var parameters = new AutoNamingDynamicParameters();
            QueryTree rootQueryNode = null;
            if (predicateArray != null) {
                this.AddWhereClause(predicateArray, whereSql, parameters, new DefaultAliasProvider(), ref rootQueryNode);
            }

            if (rootQueryNode == null) {
                // the where clauses were all on the root table
                return new SqlWriterResult(this.GetSimpleDeleteQuery<T>(whereSql), parameters);
            }

            // cross table where clause
            return new SqlWriterResult(this.GetMultiTableDeleteQuery<T>(whereSql, rootQueryNode, new DefaultAliasProvider()), parameters);
        }

        private string GetMultiTableDeleteQuery<T>(StringBuilder whereSql, QueryTree rootQueryNode, IAliasProvider aliasProvider) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();

            sql.Append("delete t from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" as t");

            foreach (var node in rootQueryNode.Children) {
                this.AddNode(node.Value, sql, aliasProvider);
            }

            sql.Append(whereSql);
            return sql.ToString();
        }

        private string GetSimpleDeleteQuery<T>(StringBuilder whereSql) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();

            sql.Append("delete from ");
            this.Dialect.AppendQuotedTableName(sql, map);

            sql.Append(whereSql);
            return sql.ToString();
        }
    }
}