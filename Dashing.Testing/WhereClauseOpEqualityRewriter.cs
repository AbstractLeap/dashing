namespace Dashing.Testing {
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Dashing.Extensions;

    public class WhereClauseOpEqualityRewriter : ExpressionVisitor {
        public Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> expression) {
            return Expression.Lambda<Func<T, bool>>(this.Visit(expression.Body), expression.Parameters.First());
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.Equal 
                && !node.Left.Type.IsValueType() // only change entity expressions
                && node.Left.Type != typeof(string) // ignore strings as well
                && !IsNullConstant(node.Left) // don't re-write null checks
                && !IsNullConstant(node.Right) // don't re-write null checks
            ) {
                return Expression.Call(node.Left, "Equals", new Type[0], node.Right);
            }

            return base.VisitBinary(node);
        }

        private static bool IsNullConstant(Expression expression) {
            return expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null;
        }
    }
}