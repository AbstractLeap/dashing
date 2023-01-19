namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Dashing.Configuration;
    using Dashing.Extensions;

    internal class OuterJoinDisjunctionTransformer : ExpressionVisitor {
        private readonly IConfiguration configuration;

        private bool foundNonRootDisjunction;

        private bool hasUnexpectedBehaviour;

        private Expression topLevelOrElseExpression;

        private readonly IList<Expression> disjunctions;

        private int currentDisjunctionIndex;

        public OuterJoinDisjunctionTransformer(IConfiguration configuration) {
            this.configuration = configuration;
            this.disjunctions = new List<Expression>();
        }

        private void ResetVariables() {
            this.foundNonRootDisjunction = false;
            this.hasUnexpectedBehaviour = false;
            this.topLevelOrElseExpression = null;
            this.currentDisjunctionIndex = 0;
            this.disjunctions.Clear();
        }

        public OuterJoinDisjunctionResult<T> AttemptGetOuterJoinDisjunctions<T>(Expression<Func<T, bool>> whereClause) {
            this.ResetVariables();
            var expression = this.Visit(whereClause);
            if (!this.disjunctions.Any() || this.hasUnexpectedBehaviour) {
                return OuterJoinDisjunctionResult<T>.None;
            }

            var expressions = new List<Expression<Func<T, bool>>> {
                                                                      (Expression<Func<T, bool>>)expression
                                                                  };
            while (!this.hasUnexpectedBehaviour && this.currentDisjunctionIndex < this.disjunctions.Count) {
                expressions.Add((Expression<Func<T, bool>>)this.Visit(whereClause));
            }

            if (this.hasUnexpectedBehaviour || !this.foundNonRootDisjunction) {
                return OuterJoinDisjunctionResult<T>.None;
            }

            return new OuterJoinDisjunctionResult<T>(expressions);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node) {
            if (node.Method.Name == "Any") {
                // predicates inside an Any clause are effectively a new context (they're inside an exists (select ...) clause so shouldn't be taken in to account here
                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node) {
            if (node.NodeType == ExpressionType.OrElse) {
                if (this.topLevelOrElseExpression != null) {
                    if (!ReferenceEquals(this.topLevelOrElseExpression, node)) {
                        this.hasUnexpectedBehaviour = true; // at the moment we only support a single collection of OrElse
                        return node;
                    }

                    // we're iterating through the disjunctions now
                    var expression = this.disjunctions[this.currentDisjunctionIndex++];
                    if (this.IsNonRootMemberAccess(expression)) {
                        this.foundNonRootDisjunction = true;
                    }

                    return expression;
                }

                // navigate down the OrElse's collecting the leaf nodes
                this.VisitOrElse(node);
                if (!this.hasUnexpectedBehaviour && this.disjunctions.Any()) {
                    this.topLevelOrElseExpression = node;
                    return this.VisitBinary(node); // returns the first expression from the disjunction
                }
            }

            return base.VisitBinary(node);
        }

        private Expression VisitOrElse(BinaryExpression node) {
            this.TryCollectDisjunctions(node.Left);
            if (!this.hasUnexpectedBehaviour) {
                this.TryCollectDisjunctions(node.Right);
            }

            return node;
        }

        private void TryCollectDisjunctions(Expression node) {
            if (node is BinaryExpression leftBinaryExpression) {
                if (IsComparisonExpression(leftBinaryExpression)) {
                    this.disjunctions.Add(leftBinaryExpression);
                }
                else if (leftBinaryExpression.NodeType == ExpressionType.OrElse) {
                    this.VisitOrElse(leftBinaryExpression);
                }
                else {
                    this.hasUnexpectedBehaviour = true; // we don't support anything else here
                }
            }
            else if (node is UnaryExpression) {
                this.disjunctions.Add(node);
            }
            else if (node is MemberExpression) {
                this.disjunctions.Add(node);
            }
            else {
                this.hasUnexpectedBehaviour = true;
            }
        }

        private static bool IsComparisonExpression(BinaryExpression node) {
            switch (node.NodeType) {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                case ExpressionType.Not:
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     Indicates the expression is a member access expression that accesses a non-root property
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private bool IsNonRootMemberAccess(Expression expression) {
            if (expression is BinaryExpression binaryExpression) {
                if (binaryExpression.NodeType == ExpressionType.OrElse || binaryExpression.NodeType == ExpressionType.AndAlso) {
                    this.hasUnexpectedBehaviour = true;
                    return false; // we should haven't any of these here
                }

                return this.IsNonRootMemberAccess(binaryExpression.Left) || this.IsNonRootMemberAccess(binaryExpression.Right);
            }

            if (expression is UnaryExpression unaryExpression) {
                return this.IsNonRootMemberAccess(unaryExpression.Operand);
            }

            if (expression is MemberExpression memberExpression) {
                if (memberExpression.Expression == null) {
                    return false; // this is a static property accesor
                }

                if (memberExpression.Expression.NodeType == ExpressionType.Parameter) {
                    return false; // this is a root where clause
                }

                if (memberExpression.Expression is MemberExpression parentMemberExpression) { // could this be an explicit cast?
                    // handle nullable<> HasValue and Value access
                    if (memberExpression.Member.DeclaringType != null && (memberExpression.Member.DeclaringType.IsGenericType() && memberExpression.Member.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                        return this.IsNonRootMemberAccess(parentMemberExpression); // we ask the same of the nullable property
                    }

                    if (parentMemberExpression.Expression.NodeType != ExpressionType.Parameter && IsMemberExpressionForParameter(parentMemberExpression)) { // we're definitely away from the root
                        return true;
                    }

                    if (parentMemberExpression.Expression.NodeType == ExpressionType.Parameter) {
                        var map = this.configuration.GetMap(memberExpression.Member.DeclaringType); // this throws if not mapped

                        if (!map.Columns.TryGetValue(memberExpression.Member.Name, out var column) || column.IsIgnored) {
                            throw new InvalidOperationException($"Property {memberExpression.Member.Name} not mapped");
                        }

                        return !column.IsPrimaryKey;
                    }
                }
            }

            return false;
        }

        private static bool IsMemberExpressionForParameter(MemberExpression memberExpression) {
            if (memberExpression.Expression.NodeType == ExpressionType.Parameter) {
                return true;
            }

            if (memberExpression.Expression is MemberExpression parentMemberExpression) {
                return IsMemberExpressionForParameter(parentMemberExpression);
            }

            return false;
        }
    }

    internal class OuterJoinDisjunctionResult<T> {
        public OuterJoinDisjunctionResult(IEnumerable<Expression<Func<T, bool>>> unionWhereClauses) {
            this.UnionWhereClauses = unionWhereClauses;
            this.ContainsOuterJoinDisjunction = true;
        }

        public OuterJoinDisjunctionResult() { }

        public bool ContainsOuterJoinDisjunction { get; }

        public IEnumerable<Expression<Func<T, bool>>> UnionWhereClauses { get; }

        public static OuterJoinDisjunctionResult<T> None = new OuterJoinDisjunctionResult<T>();
    }
}