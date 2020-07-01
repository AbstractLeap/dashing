namespace Dashing.Engine.DML {
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class OrderClauseWriter : MemberExpressionFetchNodeVisitor, IOrderClauseWriter {
        private readonly IConfiguration configuration;

        private readonly ISqlDialect dialect;

        public OrderClauseWriter(IConfiguration configuration, ISqlDialect dialect) {
            this.configuration = configuration;
            this.dialect = dialect;
        }

        public string GetOrderClause<T>(
            OrderClause<T> clause, 
            QueryTree rootQueryNode,
            IAliasProvider aliasProvider, 
            out bool isRootPrimaryKeyClause) {
            return this.GetOrderClauseInner(clause, rootQueryNode, aliasProvider, null, null, out isRootPrimaryKeyClause);
        }

        public string GetOrderClause<T>(
            OrderClause<T> clause,
            QueryTree rootQueryNode,
            IAliasProvider aliasProvider,
            Func<IColumn, BaseQueryNode, string> aliasRewriter,
            Func<IColumn, BaseQueryNode, string> nameRewriter,
            out bool isRootPrimaryKeyClause) {
            if (aliasRewriter == null) {
                throw new ArgumentNullException("aliasRewriter");
            }

            if (nameRewriter == null) {
                throw new ArgumentNullException("nameRewriter");
            }

            return this.GetOrderClauseInner(clause, rootQueryNode, aliasProvider, aliasRewriter, nameRewriter, out isRootPrimaryKeyClause);
        }

        private string GetOrderClauseInner<T>(OrderClause<T> clause, QueryTree rootQueryNode, IAliasProvider aliasProvider, Func<IColumn, BaseQueryNode, string> aliasRewriter, Func<IColumn, BaseQueryNode, string> nameRewriter, out bool isRootPrimaryKeyClause) {
            var lambdaExpression = clause.Expression as LambdaExpression;
            if (lambdaExpression == null) {
                throw new InvalidOperationException("OrderBy clauses must be LambdaExpressions");
            }

            var node = this.VisitExpression(lambdaExpression.Body, rootQueryNode);
            var sb = new StringBuilder();
            if (node == null) {
                var column = this.configuration.GetMap<T>().Columns[((MemberExpression)lambdaExpression.Body).Member.Name];
                this.dialect.AppendQuotedName(sb, nameRewriter != null ? nameRewriter(column, node) : column.DbName);
                sb.Append(" ").Append(clause.Direction == ListSortDirection.Ascending ? "asc" : "desc");
                isRootPrimaryKeyClause = column.IsPrimaryKey;
            }
            else {
                var column = node.GetMapForNode()
                                 .Columns[((MemberExpression)lambdaExpression.Body).Member.Name];
                if (node is QueryNode queryNode) {
                    isRootPrimaryKeyClause = false;
                    if (queryNode.Column.Relationship != RelationshipType.ManyToOne && queryNode.Column.Relationship != RelationshipType.OneToOne) {
                        throw new NotSupportedException();
                    }
                }
                else {
                    isRootPrimaryKeyClause = column.IsPrimaryKey;
                }

                sb.Append(aliasRewriter != null ? aliasRewriter(column, node) : aliasProvider.GetAlias(node)).Append(".");
                this.dialect.AppendQuotedName(sb, nameRewriter != null ? nameRewriter(column, node) : column.DbName);
                sb.Append(" ").Append(clause.Direction == ListSortDirection.Ascending ? "asc" : "desc");
            }

            return sb.ToString();
        }
    }
}