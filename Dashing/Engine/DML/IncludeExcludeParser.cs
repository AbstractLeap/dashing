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

        public void ParseExpression<TBase>(Expression expression, FetchNode rootNode, bool isInclude) {
            var lambda = expression as LambdaExpression;
            if (lambda == null) {
                throw new InvalidOperationException("Include and Exclude expressions must be LambdaExpressions");
            }

            var node = this.VisitExpression(lambda.Body, rootNode);
            IMap map;
            if (ReferenceEquals(node, rootNode)) {
                map = this.configuration.GetMap<TBase>();
            }
            else {
                if (node.Column.Relationship == RelationshipType.ManyToOne) {
                    map = node.Column.ParentMap;
                }
                else if (node.Column.Relationship == RelationshipType.OneToOne) {
                    map = node.Column.OppositeColumn.Map;
                }
                else {
                    throw new NotSupportedException("Include/Exclude clauses can only use Many to One and One to One relationships");
                }
            }

            var column = map.Columns[((MemberExpression)lambda.Body).Member.Name];
            if (column.Relationship != RelationshipType.None) {
                throw new NotSupportedException("Include/Exclude clauses must end with a property that is a non-relationship type property e.g. a string");
            }

            if (isInclude && node.IncludedColumns == null) {
                node.IncludedColumns = new List<IColumn>();
            }
            else if (!isInclude && node.ExcludedColumns == null) {
                node.ExcludedColumns = new List<IColumn>();
            }

            (isInclude
                 ? node.IncludedColumns
                 : node.ExcludedColumns).Add(column);
        }
    }
}