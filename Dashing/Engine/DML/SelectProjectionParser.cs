namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    internal class SelectProjectionParser<TBase> : ExpressionVisitorBase<Action<BaseQueryNode>> {
        private readonly IConfiguration configuration;

        private QueryTree rootQueryNode;

        public SelectProjectionParser(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public void ParseExpression<TProjection>(Expression<Func<TBase, TProjection>> expression, QueryTree rootQueryNode) {
            this.rootQueryNode = rootQueryNode;
            this.Visit(expression);
        }

        protected override void VisitMember(Context context, MemberExpression node) {
            BaseQueryNode mapQueryNode = null;
            this.VisitMember(node,
                x => mapQueryNode = x);
            if (!context.Parent.IsExpressionOf(ExpressionType.MemberAccess)) {
                // we're at the end
                var map = mapQueryNode.GetMapForNode();
                if (mapQueryNode is QueryNode queryNode)  {
                    if (queryNode.Column.Relationship != RelationshipType.ManyToOne && queryNode.Column.Relationship != RelationshipType.OneToOne) {
                        throw new NotSupportedException("Include/Exclude clauses can only use Many to One and One to One relationships");
                    }
                }
                
                if (map.Columns.TryGetValue(node.Member.Name, out var column)) {
                    if (column.Relationship == RelationshipType.None) {
                        // add to the included columns for this queryNode
                        mapQueryNode.AddIncludedColumn(column);
                    }
                    else if (column.Relationship == RelationshipType.ManyToOne || column.Relationship == RelationshipType.OneToOne) {
                        // add a new fetch queryNode for this column
                        if (!mapQueryNode.Children.ContainsKey(column.Name)) {
                            mapQueryNode.AddChild(column, true);
                        }
                    }
                    else {
                        throw new NotSupportedException($"Unable to project OneToMany relationships - {column.Name}");
                    }
                }
                else {
                    throw new InvalidOperationException($"Unable to find column {node.Member.Name} in projection");
                }
            }
            else {
                // we're not at the end of the member access
                var declaringType = node.Member.DeclaringType;
                if (!this.configuration.HasMap(declaringType)) {
                    throw new InvalidOperationException($"Type not mapped: {declaringType.FullName}");
                }

                var map = this.configuration.GetMap(declaringType);
                if (!mapQueryNode.Children.ContainsKey(node.Member.Name)) {
                    context.State(mapQueryNode.AddChild(map.Columns[node.Member.Name], true));
                }
                else {
                    context.State(mapQueryNode.Children[node.Member.Name]);
                }
            }
        }

        protected override void VisitParameter(Context context, ParameterExpression node) {
            context.State(this.rootQueryNode);
        }

        protected override void VisitLambda(Context context, LambdaExpression node) {
            this.VisitLambda(node, null, null, true, false);
        }
    }
}