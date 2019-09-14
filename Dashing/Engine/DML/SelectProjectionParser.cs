namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    internal class SelectProjectionParser<TBase> : ExpressionVisitorBase<Action<FetchNode>> {
        private readonly IConfiguration configuration;

        private FetchNode rootNode;

        public SelectProjectionParser(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public void ParseExpression<TProjection>(Expression<Func<TBase, TProjection>> expression, FetchNode rootNode) {
            this.rootNode = rootNode;
            this.Visit(expression);
        }

        protected override void VisitMember(Context context, MemberExpression node)
        {
            FetchNode fetchNode = null;
            this.VisitMember(node, x => fetchNode = x);
            if (!context.Parent.IsExpressionOf(ExpressionType.MemberAccess)) {
                // we're at the end
                if (ReferenceEquals(this.rootNode, fetchNode)) {
                    var map = this.configuration.GetMap<TBase>();
                    if (map.Columns.TryGetValue(node.Member.Name, out var column)) {
                        if (column.Relationship == RelationshipType.None) {
                            if (fetchNode.IncludedColumns == null) {
                                fetchNode.IncludedColumns = new List<IColumn>();
                            }

                            fetchNode.IncludedColumns.Add(column);
                        } else if (column.Relationship == RelationshipType.ManyToOne || column.Relationship == RelationshipType.OneToOne) {
                            fetchNode.AddChild(column, true);
                        }
                        else {
                            throw new NotSupportedException($"Unable to project OneToMany relationships - {column.Name}");
                        }
                    }
                }

                
            }
        }

        protected override void VisitParameter(Context context, ParameterExpression node) {
            context.State(this.rootNode);
        }

        protected override void VisitLambda(Context context, LambdaExpression node) {
            this.VisitLambda(node, null, null, true, false);
        }
    }
}