namespace Dashing.Engine.DML {
    using System;
    using System.Linq.Expressions;

    abstract class MemberExpressionFetchNodeVisitor {
        protected BaseQueryNode VisitExpression(Expression expr, QueryTree rootQueryNode) {
            var memberExpr = expr as MemberExpression;
            if (memberExpr == null) {
                throw new InvalidOperationException("Order/Include/Exclude clauses must contain MemberExpressions");
            }

            if (memberExpr.Expression.NodeType == ExpressionType.Parameter) {
                // we're at the bottom
                return rootQueryNode; // this should be the root queryNode
            }

            // not at the bottom, find the child and return that
            var parentNode = this.VisitExpression(memberExpr.Expression, rootQueryNode);
            if (parentNode == null) {
                throw new InvalidOperationException("You must Fetch a relationship if you want to use it in an order by or include/exclude clause");
            }

            var baseExpr = memberExpr.Expression as MemberExpression;
            if (baseExpr == null) {
                throw new InvalidOperationException("Order/Include/Exclude clauses must contain MemberExpressions");
            }

            if (!parentNode.Children.ContainsKey(baseExpr.Member.Name)) {
                throw new InvalidOperationException($"You must Fetch {baseExpr.Member.Name} if you wish to you it in an Order/Include/Exclude clause");
            }

            return parentNode.Children[baseExpr.Member.Name];
        }
    }
}