namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Engine.DML;

    class ProjectionExpressionRewriter<TBase, TProjection> : ExpressionVisitor
        where TBase : class, new() {
        private readonly ProjectedSelectQuery<TBase, TProjection> query;

        private readonly FetchNode rootNode;

        private readonly IList<Type> types;

        public ProjectionExpressionRewriter(ProjectedSelectQuery<TBase, TProjection> query, FetchNode rootNode) {
            this.query = query;
            this.rootNode = rootNode;
            this.types = new List<Type>();
        }

        public DelegateProjectionResult<TProjection> Rewrite() {
            var expr = this.Visit(this.query.ProjectionExpression);
            return new DelegateProjectionResult<TProjection>(this.types.ToArray(), (Func<object[], TProjection>)((LambdaExpression)expr).Compile());
        }

        protected override Expression VisitMember(MemberExpression node) {
            if (node.Expression.NodeType != ExpressionType.Parameter) {
                // we're accessing something above the root
            }

            return node;
        }

        private FetchNode VisitMember(Expression expression) {
            if (expression.NodeType == ExpressionType.Parameter) {
                return this.rootNode;
            }

            if (expression is MemberExpression memberExpression) {
                var node = this.VisitMember(memberExpression.Expression);
            }

            throw new NotSupportedException();
        }
    }
}