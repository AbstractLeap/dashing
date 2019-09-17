namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.DML;
    using Dashing.Extensions;

    class ProjectionExpressionRewriter<TBase, TProjection> : ExpressionVisitor
        where TBase : class, new() {
        private readonly IConfiguration configuration;

        private readonly ProjectedSelectQuery<TBase, TProjection> query;

        private readonly FetchNode rootNode;

        private readonly IDictionary<FetchNode, FetchNodeLookupValue> fetchNodeLookup;

        private readonly ParameterExpression parameterExpression;

        public ProjectionExpressionRewriter(IConfiguration configuration, ProjectedSelectQuery<TBase, TProjection> query, FetchNode rootNode) {
            this.configuration = configuration;
            this.query = query;
            this.rootNode = rootNode;
            this.fetchNodeLookup = new Dictionary<FetchNode, FetchNodeLookupValue>();
            this.parameterExpression = Expression.Parameter(typeof(object[]));
        }

        public DelegateProjectionResult<TProjection> Rewrite() {
            var expr = this.Visit(this.query.ProjectionExpression);
            var newLambda = Expression.Lambda(((LambdaExpression)expr).Body, this.parameterExpression);
            return new DelegateProjectionResult<TProjection>(
                this.fetchNodeLookup.Values.OrderBy(l => l.Idx).Select(l => l.ConversionType).ToArray(), 
                (Func<object[], TProjection>)newLambda.Compile());
        }

        protected override Expression VisitMember(MemberExpression node) {
            var fetchNode = this.VisitMember(node);
            var isRootNode = ReferenceEquals(fetchNode, this.rootNode);
            if (!this.fetchNodeLookup.TryGetValue(fetchNode, out var lookup)) {
                lookup = new FetchNodeLookupValue {
                                                      Idx = this.fetchNodeLookup.Count,
                                                      ConversionType = isRootNode
                                                                           ? typeof(TBase)
                                                                           : (IsRelationshipAccess()
                                                                                  ? fetchNode.Column.Type
                                                                                  : node.Member.DeclaringType)
                                                  };
                this.fetchNodeLookup.Add(fetchNode, lookup);
            }
            
            var convertExpression = Expression.Convert(
                Expression.ArrayAccess(this.parameterExpression, Expression.Constant(lookup.Idx)),
                lookup.ConversionType);
            if (!isRootNode && IsRelationshipAccess()) {
                return convertExpression;
            }

            return Expression.MakeMemberAccess(convertExpression, node.Member);

            bool IsRelationshipAccess() {
                return fetchNode.Column.Name == node.Member.Name;
            }
        }

        private FetchNode VisitMember(Expression expression) {
            if (expression.NodeType == ExpressionType.Parameter) {
                return this.rootNode;
            }

            if (expression is MemberExpression memberExpression) {
                var fetchNode = this.VisitMember(memberExpression.Expression);
                var map = ReferenceEquals(this.rootNode, fetchNode)
                              ? this.configuration.GetMap<TBase>()
                              : (fetchNode.Column.Relationship == RelationshipType.ManyToOne
                                     ? fetchNode.Column.ParentMap
                                     : (fetchNode.Column.Relationship == RelationshipType.OneToOne
                                            ? fetchNode.Column.OppositeColumn.Map
                                            : throw new NotSupportedException("Include/Exclude clauses can only use Many to One and One to One relationships")));
                if (map.Columns.TryGetValue(memberExpression.Member.Name, out var column)) {
                    if (column.Relationship == RelationshipType.None) {
                        return fetchNode; // this is at the bottom anyway, we don't need to specify a different parameter
                    }

                    if (column.Relationship == RelationshipType.ManyToOne || column.Relationship == RelationshipType.OneToOne) {
                        return fetchNode.Children[column.Name];
                    }
                }
                else {
                    throw new InvalidOperationException($"Unable to find column to project");
                }

                //if (ReferenceEquals(node, this.rootNode)) {
                //    var map = this.configuration.GetMap<TBase>();
                //    if (map.Columns.TryGetValue(memberExpression.Member.Name, out var column)) {
                //        if (column.Relationship == RelationshipType.None) {
                //            return node; // this is at the bottom anyway, we don't need to specify a different parameter
                //        }
                //        else if (column.Relationship == RelationshipType.ManyToOne || column.Relationship == RelationshipType.OneToOne) {
                //            return node.Children[column.Name];
                //        }
                //    } else {
                //        throw new InvalidOperationException($"Unable to find column to project");
                //    }
                //}
                //else {
                //    return node.Children[memberExpression.Member.Name];
                //}
            }

            throw new NotSupportedException();
        }

        class FetchNodeLookupValue {
            public int Idx { get; set; }

            public Type ConversionType { get; set; }
        }
    }
}