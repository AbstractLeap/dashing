namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class BaseWriter {
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
                sql.Append(" where ");
                sql.Append(result.Sql);
            }

            rootNode = result.FetchTree;
            return result.Parameters;
        }

        public bool AddOrderByClause<T>(
            Queue<OrderClause<T>> orderClauses,
            StringBuilder sql,
            FetchNode rootNode,
            Func<IColumn, FetchNode, string> aliasRewriter = null,
            Func<IColumn, FetchNode, string> nameRewriter = null) {
            if (orderClauses.Count == 0) {
                return false;
            }

            sql.Append(" order by ");
            var orderClauseWriter = new OrderClauseWriter(this.Configuration, this.Dialect);
            var containsRootPrimaryKeyClause = false;
            while (orderClauses.Count > 0) {
                var isRootPrimaryKeyClause = false;
                if (aliasRewriter == null && nameRewriter == null) {
                    sql.Append(orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), rootNode, out isRootPrimaryKeyClause));
                }
                else {
                    sql.Append(
                        orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), rootNode, aliasRewriter, nameRewriter, out isRootPrimaryKeyClause));
                }

                if (orderClauses.Count > 0) {
                    sql.Append(", ");
                }

                if (isRootPrimaryKeyClause) {
                    containsRootPrimaryKeyClause = true;
                }
            }

            return containsRootPrimaryKeyClause;
        }
    }
}