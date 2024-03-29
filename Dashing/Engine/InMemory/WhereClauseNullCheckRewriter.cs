﻿namespace Dashing.Engine.InMemory {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Dashing.Extensions;

    public class WhereClauseNullCheckRewriter : ExpressionVisitor {
        private IList<Expression> nullCheckExpressions;

        private bool treeHasParameter;

        private bool isNullCheck;

        private bool containsNullable;

        public Expression<Func<T, bool>> Rewrite<T>(Expression<Func<T, bool>> expression) {
            this.ResetVariables();
            var expr = this.Visit(expression.Body);
            if (Enumerable.Any<Expression>(this.nullCheckExpressions)) {
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

                var leftHandSideIsNull = this.isNullCheck;
                var leftHandSideContainsNullable = this.containsNullable;
                var leftHandSideNullCheckExpressions = this.nullCheckExpressions;
                var leftHandSideHasParameter = this.treeHasParameter;
                this.ResetVariables();

                // visit right hand side
                if (node.Right.NodeType == ExpressionType.Convert) {
                    this.Visit(((UnaryExpression)node.Right).Operand);
                }
                else {
                    this.Visit(node.Right);
                }

                var rightHandSideIsNull = this.isNullCheck;
                var rightHandSideContainsNullable = this.containsNullable;
                var rightHandSideNullCheckExpressions = this.nullCheckExpressions;
                var rightHandSideHasParameter = this.treeHasParameter;

                if (leftHandSideIsNull && !rightHandSideContainsNullable && rightHandSideNullCheckExpressions.Count > 0) {
                    rightHandSideNullCheckExpressions.RemoveAt(
                        rightHandSideNullCheckExpressions.Count - 1);
                }

                if (rightHandSideIsNull && !leftHandSideContainsNullable && leftHandSideNullCheckExpressions.Count > 0) {
                    leftHandSideNullCheckExpressions.RemoveAt(
                        leftHandSideNullCheckExpressions.Count - 1);
                }
                

                var combined = CombineExpressions(node, leftHandSideNullCheckExpressions.Union(rightHandSideNullCheckExpressions));
                this.nullCheckExpressions.Clear();
                return combined;
            }

            if (isInAndOrOrExpression) {
                var leftExpr = this.CombineExpressions(this.Visit(node.Left));
                var rightExpr = this.CombineExpressions(this.Visit(node.Right));
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

                if (propInfo.PropertyType.IsNullable()) {
                    this.containsNullable = true;
                }

                if (propInfo.PropertyType.IsClass() && propInfo.PropertyType != typeof(string)) {
                    if (Enumerable.Any<Expression>(this.nullCheckExpressions)) {
                        // we just use the last null check expression and add to that
                        this.nullCheckExpressions.Add(
                            Expression.NotEqual(
                                Expression.Property(((BinaryExpression)Enumerable.Last<Expression>(this.nullCheckExpressions)).Left, propInfo),
                                Expression.Constant(null)));
                    }
                    else {
                        this.nullCheckExpressions.Add(
                            Expression.NotEqual(Expression.Property(node.Expression, propInfo), Expression.Constant(null)));
                    }
                }
            }

            // return the expression
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node) {
            if (node.Value == null) {
                this.isNullCheck = true;
            }

            return base.VisitConstant(node);
        }

        private void ResetVariables() {
            this.nullCheckExpressions = new List<Expression>();
            this.treeHasParameter = false;
            this.isNullCheck = false;
            this.containsNullable = false;
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

        private static Expression CombineExpressions(Expression exp, IEnumerable<Expression> expressions) {
            if (!expressions.Any()) {
                return exp;
            }

            if (expressions.Count() == 1) {
                var expr = Expression.AndAlso(Enumerable.First<Expression>(expressions), exp);
                return expr;
            }

            var combinedExpr = Expression.AndAlso(Enumerable.First<Expression>(expressions), Enumerable.ElementAt<Expression>(expressions, 1));
            for (var i = 2; i < expressions.Count(); i++) {
                combinedExpr = Expression.AndAlso(combinedExpr, Enumerable.ElementAt<Expression>(expressions, i));
            }

            return Expression.AndAlso(combinedExpr, exp);
        }

        private Expression CombineExpressions(Expression exp) {
            var combined = CombineExpressions(exp, this.nullCheckExpressions);
            this.nullCheckExpressions.Clear();
            return combined;
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