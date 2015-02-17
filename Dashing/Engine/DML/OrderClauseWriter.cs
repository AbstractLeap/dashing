namespace Dashing.Engine.DML {
    using System;
    using System.Linq.Expressions;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class OrderClauseWriter : IOrderClauseWriter {
        private readonly IConfiguration configuration;

        private readonly ISqlDialect dialect;

        public OrderClauseWriter(IConfiguration configuration, ISqlDialect dialect) {
            this.configuration = configuration;
            this.dialect = dialect;
        }

        public string GetOrderClause<T>(OrderClause<T> clause, FetchNode rootNode) {
            var lambdaExpression = clause.Expression as LambdaExpression;
            if (lambdaExpression == null) {
                throw new InvalidOperationException("OrderBy clauses must be LambdaExpressions");
            }

            var node = this.VisitOrderClause(lambdaExpression.Body, rootNode);
            var sb = new StringBuilder();
            if (node == null) {
                this.dialect.AppendQuotedName(
                    sb,
                    this.configuration.GetMap<T>().Columns[((MemberExpression)lambdaExpression.Body).Member.Name].DbName);
                sb.Append(" ").Append(clause.Direction == System.ComponentModel.ListSortDirection.Ascending ? "asc" : "desc");
            }
            else {
                sb.Append(node.Alias).Append(".");
                if (Object.ReferenceEquals(node, rootNode)) {
                    this.dialect.AppendQuotedName(sb, this.configuration.GetMap<T>().Columns[((MemberExpression)lambdaExpression.Body).Member.Name].DbName);
                }
                else {
                    this.dialect.AppendQuotedName(sb, node.Column.ParentMap.Columns[((MemberExpression)lambdaExpression.Body).Member.Name].DbName);
                }

                sb.Append(" ").Append(clause.Direction == System.ComponentModel.ListSortDirection.Ascending ? "asc" : "desc");
            }

            return sb.ToString();
        }

        private FetchNode VisitOrderClause(Expression expr, FetchNode rootNode) {
            var memberExpr = expr as MemberExpression;
            if (memberExpr == null) {
                throw new InvalidOperationException("OrderBy clauses must contain MemberExpressions");
            }

            if (memberExpr.Expression.NodeType == ExpressionType.Parameter) {
                // we're at the bottom
                return rootNode;
            }
            else {
                // we're not at the bottom, find the child and return that
                var parentNode = this.VisitOrderClause(memberExpr.Expression, rootNode);
                if (parentNode == null) {
                    throw new InvalidOperationException("You must Fetch a relationship if you wish to OrderBy it");
                }

                var baseExpr = memberExpr.Expression as MemberExpression;
                if (baseExpr == null) {
                    throw new InvalidOperationException("OrderBy clauses must contain MemberExpressions");
                }

                if (!parentNode.Children.ContainsKey(baseExpr.Member.Name)) {
                    throw new InvalidOperationException("You must Fetch a relationship if you wish to OrderBy it");
                }

                return parentNode.Children[baseExpr.Member.Name];
            }
        }
    }
}