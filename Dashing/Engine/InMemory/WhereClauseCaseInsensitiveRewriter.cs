namespace Dashing.Engine.InMemory {
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public class WhereClauseCaseInsensitiveRewriter : ExpressionVisitor {
        public Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> expression) {
            return Expression.Lambda<Func<T, bool>>(this.Visit(expression.Body), expression.Parameters.First());
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            Expression memberExpr = null;
            Expression valuesExpr = null;

            switch (node.Method.Name) {
                case "Contains":
                    if (node.Method.DeclaringType == typeof(string)) {
                        // string.Contains
                        memberExpr = node.Object;
                        valuesExpr = node.Arguments[0];
                        return Expression.Call(
                            memberExpr,
                            "Contains",
                            new[] { typeof(string), typeof(StringComparison) },
                            valuesExpr,
                            Expression.Constant(StringComparison.CurrentCultureIgnoreCase));
                    }
                    if (node.Method.DeclaringType == typeof(Enumerable)) {
                        // static method
                        memberExpr = node.Arguments[1] as MemberExpression;
                        valuesExpr = node.Arguments[0];
                    }
                    else {
                        // contains on IList
                        memberExpr = node.Arguments[0] as MemberExpression;
                        valuesExpr = node.Object;
                    }
                    break;

                case "StartsWith":
                case "EndsWith":
                    memberExpr = node.Object;
                    valuesExpr = node.Arguments[0];
                    break;

                case "Any":
                    // TODO: probably check this?
                    memberExpr = node.Arguments[0] as MemberExpression;
                    valuesExpr = node.Arguments[1];
                    break;
            }

            return node;
        }
    }
}