namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    public class BaseWriter {
        protected internal ISqlDialect Dialect { get; set; }

        protected internal IConfiguration Configuration { get; set; }

        public BaseWriter(ISqlDialect dialect, IConfiguration config) {
            this.Dialect = dialect;
            this.Configuration = config;
        }

        protected void AddNode(QueryNode queryNode, StringBuilder tableSql, IAliasProvider aliasProvider) {
            var map = queryNode.Column.GetMapOfColumnType();
            this.AddTableSqlForNode(queryNode, map, tableSql, aliasProvider);
            foreach (var child in queryNode.Children) {
                this.AddNode(child.Value, tableSql, aliasProvider);
            }
        }

        protected void AddNode(QueryNode queryNode, StringBuilder tableSql, StringBuilder columnSql, IAliasProvider aliasProvider, bool selectQueryFetchAllProperties, bool isProjectedQuery) {
            // add this queryNode and then it's children
            // add table sql
            var map = queryNode.GetMapForNode();
            this.AddTableSqlForNode(queryNode, map, tableSql, aliasProvider);

            // add the columns
            if (queryNode.IsFetched) {
                var columns = queryNode.GetSelectedColumns();
                columns = columns.Where(
                    c => !queryNode.Children.ContainsKey(c.Name) || !queryNode.Children[c.Name]
                                                                    .IsFetched);
                foreach (var columnEntry in columns.AsSmartEnumerable()) {
                    columnSql.Append(", ");
                    this.AddColumn(columnSql, columnEntry.Value, aliasProvider.GetAlias(queryNode));
                }
            }

            // add its children
            foreach (var child in queryNode.Children) {
                this.AddNode(child.Value, tableSql, columnSql, aliasProvider, selectQueryFetchAllProperties, isProjectedQuery);
            }
        }

        private void AddTableSqlForNode(QueryNode queryNode, IMap map, StringBuilder tableSql, IAliasProvider aliasProvider) {
            // if this is a non-nullable relationship and we've not already done a left join on the way to this queryNode
            // we can do an inner join
            tableSql.Append(
                queryNode.InferredInnerJoin 
                || (!queryNode.Column.IsNullable 
                    && queryNode.Column.Relationship != RelationshipType.OneToMany 
                    && (!(queryNode.Parent is QueryNode parentQueryNode) || !parentQueryNode.HasAnyNullableAncestor()))
                    ? " inner join "
                    : " left join ");
            this.Dialect.AppendQuotedTableName(tableSql, map);
            tableSql.Append(" as " + aliasProvider.GetAlias(queryNode));

            if (queryNode.Column.Relationship == RelationshipType.ManyToOne || queryNode.Column.Relationship == RelationshipType.OneToOne) {
                tableSql.Append(" on " + aliasProvider.GetAlias(queryNode.Parent) + "." + queryNode.Column.DbName + " = " + aliasProvider.GetAlias(queryNode) + "." + map.PrimaryKey.DbName);
            }
            else if (queryNode.Column.Relationship == RelationshipType.OneToMany) {
                tableSql.Append(" on " + aliasProvider.GetAlias(queryNode.Parent) + "." + queryNode.Column.Map.PrimaryKey.DbName + " = " + aliasProvider.GetAlias(queryNode) + "." + queryNode.Column.ChildColumn.DbName);
            }
        }
        
        protected void AddColumn(StringBuilder sql, IColumn column, string tableAlias = null, string columnAlias = null) {
            // add the table alias
            if (tableAlias != null) {
                sql.Append(tableAlias + ".");
            }

            // add the column name
            this.Dialect.AppendQuotedName(sql, column.DbName);

            // add a column alias if required
            if (columnAlias != null) {
                sql.Append(" as ");
                this.Dialect.AppendQuotedName(sql, columnAlias);
            }
            else if (column.DbName != column.Name && column.Relationship == RelationshipType.None) {
                sql.Append(" as ");
                this.Dialect.AppendQuotedName(sql, column.Name);
            }
        }
        
        internal void AddWhereClause<T>(IList<Expression<Func<T, bool>>> whereClauses, StringBuilder sql, AutoNamingDynamicParameters parameters, IAliasProvider aliasProvider, ref QueryTree queryTree) {
            var whereClauseWriter = new WhereClauseWriter(this.Dialect, this.Configuration);
            var result = whereClauseWriter.GenerateSql(whereClauses, queryTree, parameters, aliasProvider);
            if (result.Sql.Length > 0) {
                sql.Append(result.Sql);
            }

            queryTree = result.MapQueryTree;
        }

        protected bool AddOrderByClause<T>(Queue<OrderClause<T>> orderClauses, StringBuilder sql, QueryTree queryTree, IAliasProvider aliasProvider, Func<IColumn, BaseQueryNode, string> aliasRewriter = null, Func<IColumn, BaseQueryNode, string> nameRewriter = null) {
            if (orderClauses.Count == 0) {
                return false;
            }

            sql.Append(" order by ");
            var orderClauseWriter = new OrderClauseWriter(this.Configuration, this.Dialect);
            var containsRootPrimaryKeyClause = false;
            while (orderClauses.Count > 0) {
                var isRootPrimaryKeyClause = false;
                if (aliasRewriter == null && nameRewriter == null) {
                    sql.Append(orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), queryTree, aliasProvider, out isRootPrimaryKeyClause));
                }
                else {
                    sql.Append(orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), queryTree, aliasProvider, aliasRewriter, nameRewriter, out isRootPrimaryKeyClause));
                }

                if (orderClauses.Count > 0) {
                    sql.Append(", ");
                }

                if (isRootPrimaryKeyClause) {
                    containsRootPrimaryKeyClause = true;
                }
            }

            return containsRootPrimaryKeyClause;
        }

        /// <summary>
        ///     look up the column type and decide where to get the value from
        /// </summary>
        /// <param name="mappedColumn"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        protected object GetValueOrPrimaryKey(IColumn mappedColumn, object propertyValue) {
            switch (mappedColumn.Relationship) {
                case RelationshipType.None:
                    return propertyValue;

                case RelationshipType.ManyToOne:
                case RelationshipType.OneToOne:
                    var foreignKeyMap = this.Configuration.GetMap(mappedColumn.Type);
                    return foreignKeyMap.GetPrimaryKeyValue(propertyValue);

                default:
                    throw new NotImplementedException($"Unexpected column relationship {mappedColumn.Relationship} on entity {mappedColumn.Type.Name}.{mappedColumn.Name} in UpdateWriter");
            }
        }
    }
}