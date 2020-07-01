namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    internal class IncludeExcludeParser : MemberExpressionFetchNodeVisitor {
        private readonly IConfiguration configuration;

        public IncludeExcludeParser(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public void ParseExpression<TBase>(Expression expression, QueryTree rootQueryNode, bool isInclude) {
            var lambda = expression as LambdaExpression;
            if (lambda == null) {
                throw new InvalidOperationException("Include and Exclude expressions must be LambdaExpressions");
            }

            var node = this.VisitExpression(lambda.Body, rootQueryNode);
            var column = node.GetMapForNode()
                             .Columns[((MemberExpression)lambda.Body).Member.Name];
            if (node is QueryNode queryNode) {
                if (queryNode.Column.Relationship != RelationshipType.ManyToOne && queryNode.Column.Relationship != RelationshipType.OneToOne) {
                    throw new NotSupportedException();
                }
            }

            if (column.Relationship != RelationshipType.None) {
                throw new NotSupportedException("Include/Exclude clauses must end with a property that is a non-relationship type property e.g. a string");
            }

            if (isInclude) {
                node.AddIncludedColumn(column);
            }
            else {
                node.AddExcludedColumn(column);
            }
        }
    }
}