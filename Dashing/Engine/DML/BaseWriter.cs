namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    internal class BaseWriter {
        protected internal ISqlDialect Dialect { get; set; }

        protected internal IConfiguration Configuration { get; set; }

        public BaseWriter(ISqlDialect dialect, IConfiguration config) {
            this.Dialect = dialect;
            this.Configuration = config;
        }

        public DynamicParameters AddWhereClause<T>(IList<Expression<Func<T, bool>>> whereClauses, StringBuilder sql, ref FetchNode rootNode) {
            var whereClauseWriter = new WhereClauseWriter(this.Dialect, this.Configuration);
            var result = whereClauseWriter.GenerateSql(whereClauses, rootNode);
            if (result.Sql.Length > 0) {
                sql.Append(result.Sql);
            }

            rootNode = result.FetchTree;
            return result.Parameters;
        }

        public void AddOrderByClause<T>(Queue<OrderClause<T>> orderClauses, StringBuilder sql, FetchNode rootNode) {
            if (orderClauses.Count == 0) {
                return;
            }

            sql.Append(" order by ");
            var orderClauseWriter = new OrderClauseWriter(this.Configuration, this.Dialect);
            while (orderClauses.Count > 0) {
                sql.Append(orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), rootNode));
                if (orderClauses.Count > 0) {
                    sql.Append(", ");
                }
            }
        }
    }
}