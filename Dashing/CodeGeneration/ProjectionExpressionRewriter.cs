namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.DML;

    class ProjectionExpressionRewriter<TBase, TProjection> : ExpressionVisitor
        where TBase : class, new() {
        private readonly IConfiguration configuration;

        private readonly ProjectedSelectQuery<TBase, TProjection> query;

        private readonly FetchNode rootNode;

        private readonly IList<Type> types;

        public ProjectionExpressionRewriter(IConfiguration configuration, ProjectedSelectQuery<TBase, TProjection> query, FetchNode rootNode) {
            this.configuration = configuration;
            this.query = query;
            this.rootNode = rootNode;
            this.types = new List<Type>();
        }

        public DelegateProjectionResult<TProjection> Rewrite() {
            var expr = this.Visit(this.query.ProjectionExpression);
            return new DelegateProjectionResult<TProjection>(this.types.ToArray(), (Func<object[], TProjection>)((LambdaExpression)expr).Compile());
        }

        protected override Expression VisitMember(MemberExpression node) {
            var fetchNode = this.VisitMember(node);
            if (ReferenceEquals(this.rootNode, fetchNode)) {
                return node;
            }

            
        }

        private FetchNode VisitMember(Expression expression) {
            if (expression.NodeType == ExpressionType.Parameter) {
                return this.rootNode;
            }

            if (expression is MemberExpression memberExpression) {
                var node = this.VisitMember(memberExpression.Expression);
                if (ReferenceEquals(node, this.rootNode)) {
                    var map = this.configuration.GetMap<TBase>();
                    if (map.Columns.TryGetValue(memberExpression.Member.Name, out var column)) {
                        if (column.Relationship == RelationshipType.None) {
                            return node; // this is at the bottom anyway, we don't need to specify a different parameter
                        }
                        else if (column.Relationship == RelationshipType.ManyToOne || column.Relationship == RelationshipType.OneToOne) {
                            return node.Children[column.Name];
                        }
                    } else {
                        throw new InvalidOperationException($"Unable to find column to project");
                    }
                }
                else {
                    return node.Children[memberExpression.Member.Name];
                }
            }

            throw new NotSupportedException();
        }
    }
}