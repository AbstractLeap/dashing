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
            FetchNode rootNode = null;
            if (predicateArray != null) {
                this.AddWhereClause(predicateArray, whereSql, parameters, ref rootNode);
            }

            if (rootNode == null) {
                // the where clauses were all on the root table
                return new SqlWriterResult(this.GetSimpleDeleteQuery<T>(whereSql), parameters);
            }

            // cross table where clause
            return new SqlWriterResult(this.GetMultiTableDeleteQuery<T>(whereSql, rootNode), parameters);
        }

        private string GetMultiTableDeleteQuery<T>(StringBuilder whereSql, FetchNode rootNode) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();

            sql.Append("delete t from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append(" as t");

            foreach (var node in rootNode.Children) {
                this.AddNode(node.Value, sql);
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