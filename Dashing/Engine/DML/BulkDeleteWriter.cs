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
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();
            var parameters = new AutoNamingDynamicParameters();

            sql.Append("delete from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            if (predicates != null) {
                this.AppendPredicates(predicates, sql, parameters);
            }

            return new SqlWriterResult(sql.ToString(), parameters);
        }

        private void AppendPredicates<T>(IEnumerable<Expression<Func<T, bool>>> predicates, StringBuilder sql, AutoNamingDynamicParameters parameters) {
            var predicateArray = predicates as Expression<Func<T, bool>>[] ?? predicates.ToArray();

            if (!predicateArray.Any()) {
                return;
            }

            var whereClauseWriter = new WhereClauseWriter(this.Dialect, this.Configuration);
            var whereResult = whereClauseWriter.GenerateSql(predicateArray, null, parameters);
            if (whereResult.FetchTree != null && whereResult.FetchTree.Children.Any()) {
                throw new NotImplementedException("Dashing does not currently support where clause across tables in a delete");
            }

            sql.Append(whereResult.Sql);
        }
    }
}