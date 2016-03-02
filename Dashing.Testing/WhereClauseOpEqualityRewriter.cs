namespace Dashing.Testing {
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class WhereClauseOpEqualityRewriter : ExpressionVisitor {
        public Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> expression) {
            return Expression.Lambda<Func<T, bool>>(this.Visit(expression.Body), expression.Parameters.First());
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.Equal && !node.Left.Type.IsValueType && node.Left.Type != typeof(string)) {
                return Expression.Call(node.Left, "Equals", new Type[0], node.Right);
            }

            return base.VisitBinary(node);
        }
    }
}