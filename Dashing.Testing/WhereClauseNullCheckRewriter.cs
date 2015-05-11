namespace Dashing.Testing {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class WhereClauseNullCheckRewriter : ExpressionVisitor {
        private Queue<Expression> nullCheckExpressions;

        private bool treeHasParameter;

        public Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> expression) {
            this.ResetVariables();
            var expr = this.Visit(expression.Body);
            if (this.nullCheckExpressions.Any()) {
                return Expression.Lambda<Func<T, bool>>(this.CombineExpressions(expression.Body), expression.Parameters.First());
            }

            return Expression.Lambda<Func<T, bool>>(expr, expression.Parameters.First());
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            var isInBinaryComparisonExpression = this.IsInBinaryComparisonExpression(node.NodeType);
            var isInAndOrOrExpression = node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse;

            if (isInBinaryComparisonExpression) {
                // inside a comparison here 
                // visit left hand side
                this.ResetVariables();
                if (node.Left.NodeType == ExpressionType.Convert) {
                    this.Visit(((UnaryExpression)node.Left).Operand);
                }
                else {
                    this.Visit(node.Left);
                }

                // visit right hand side
                this.treeHasParameter = false;
                if (node.Right.NodeType == ExpressionType.Convert) {
                    this.Visit(((UnaryExpression)node.Right).Operand);
                }
                else {
                    this.Visit(node.Right);
                }

                return this.CombineExpressions(node);
            }
            if (isInAndOrOrExpression) {
                var leftExpr = this.Visit(node.Left);
                var rightExpr = this.Visit(node.Right);
                return this.ModifyExpression(leftExpr, rightExpr, node.NodeType);
            }
            // according to the WhereClauseWriter we're almost certainly inside a constant expression here so just return
            return node;
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
                    }
                    else {
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

                default:
                    throw new NotImplementedException();
            }

            this.ResetVariables();
            this.Visit(memberExpr);
            this.treeHasParameter = false;
            this.Visit(valuesExpr);
            return this.CombineExpressions(node);
        }

        protected override Expression VisitUnary(UnaryExpression node) {
            this.ResetVariables();
            this.Visit(node.Operand);
            return this.CombineExpressions(node);
        }

        protected override Expression VisitParameter(ParameterExpression node) {
            this.treeHasParameter = true;
            return node;
        }

        protected override Expression VisitMember(MemberExpression node) {
            if (node.Expression == null) {
                // static property
                return node;
            }

            // visit the expression and if subsequently treeHasParameter = true then add the null check
            this.Visit(node.Expression);
            if (this.treeHasParameter) {
                var propInfo = node.Member as PropertyInfo;
                if (propInfo == null) {
                    throw new NotImplementedException(); // as per WhereClauseWriter
                }

                if (propInfo.PropertyType.IsClass && propInfo.PropertyType != typeof(string)) {
                    if (this.nullCheckExpressions.Any()) {
                        // we just use the last null check expression and add to that
                        this.nullCheckExpressions.Enqueue(
                            Expression.NotEqual(
                                Expression.Property(((BinaryExpression)this.nullCheckExpressions.Last()).Left, propInfo),
                                Expression.Constant(null)));
                    }
                    else {
                        this.nullCheckExpressions.Enqueue(Expression.NotEqual(Expression.Property(node.Expression, propInfo), Expression.Constant(null)));
                    }
                }
            }

            // return the expression
            return node;
        }

        private void ResetVariables() {
            this.nullCheckExpressions = new Queue<Expression>();
            this.treeHasParameter = false;
        }

        private Expression ModifyExpression(Expression leftExpr, Expression rightExpr, ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal:
                    return Expression.Equal(leftExpr, rightExpr);
                case ExpressionType.GreaterThan:
                    return Expression.GreaterThan(leftExpr, rightExpr);
                case ExpressionType.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(leftExpr, rightExpr);
                case ExpressionType.LessThan:
                    return Expression.LessThan(leftExpr, rightExpr);
                case ExpressionType.LessThanOrEqual:
                    return Expression.LessThanOrEqual(leftExpr, rightExpr);
                case ExpressionType.NotEqual:
                    return Expression.NotEqual(leftExpr, rightExpr);
                case ExpressionType.AndAlso:
                    return Expression.AndAlso(leftExpr, rightExpr);
                case ExpressionType.OrElse:
                    return Expression.OrElse(leftExpr, rightExpr);
                default:
                    throw new NotImplementedException();
            }
        }

        private Expression CombineExpressions(Expression exp) {
            if (!this.nullCheckExpressions.Any()) {
                return exp;
            }

            if (this.nullCheckExpressions.Count == 1) {
                return Expression.AndAlso(this.nullCheckExpressions.Dequeue(), exp);
            }

            var combinedExpr = Expression.AndAlso(this.nullCheckExpressions.Dequeue(), this.nullCheckExpressions.Dequeue());
            while (this.nullCheckExpressions.Any()) {
                combinedExpr = Expression.AndAlso(combinedExpr, this.nullCheckExpressions.Dequeue());
            }

            return Expression.AndAlso(combinedExpr, exp);
        }

        private bool IsInBinaryComparisonExpression(ExpressionType nodeType) {
            switch (nodeType) {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return true;
                default:
                    return false;
            }
        }
    }
}