namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using TopHat.Configuration;
    using TopHat.Extensions;

    internal class BaseWriter {
        protected internal ISqlDialect Dialect { get; set; }

        protected internal IWhereClauseWriter WhereClauseWriter { get; set; }

        protected internal IDictionary<Type, IMap> Maps { get; set; }

        public BaseWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IDictionary<Type, IMap> maps) {
            this.Dialect = dialect;
            this.WhereClauseWriter = whereClauseWriter;
            this.Maps = maps;
        }

        public DynamicParameters AddWhereClause<T>(IList<Expression<Func<T, bool>>> whereClauses, StringBuilder sql, FetchNode rootNode) {
            var result = this.WhereClauseWriter.GenerateSql(whereClauses, rootNode);
            if (result.Sql.Length > 0) {
                sql.Append(result.Sql);
            }

            return result.Parameters;
        }

        public void AddOrderByClause<T>(Queue<OrderClause<T>> orderClauses, StringBuilder sql) {
            if (orderClauses.IsEmpty()) {
                return;
            }

            sql.Append(" order by ");
            foreach (var orderClause in orderClauses) {
                var lambdaExpr = orderClause.Expression as LambdaExpression;
                var memberExpr = lambdaExpr.Body as MemberExpression;
                this.Dialect.AppendQuotedName(sql, this.Maps[typeof(T)].Columns[memberExpr.Member.Name].DbName);
                sql.Append(orderClause.Direction == ListSortDirection.Ascending ? " asc, " : "desc, ");
            }

            sql.Remove(sql.Length - 2, 2);
        }
    }
}