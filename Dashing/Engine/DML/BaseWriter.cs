namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class BaseWriter {
        protected internal ISqlDialect Dialect { get; set; }

        protected internal IConfiguration Configuration { get; set; }

        public BaseWriter(ISqlDialect dialect, IConfiguration config) {
            this.Dialect = dialect;
            this.Configuration = config;
        }

        protected void AddNode(FetchNode node, StringBuilder tableSql) {
            var map = this.GetMapForNode(node);
            this.AddTableSqlForNode(node, map, tableSql);
            foreach (var child in node.Children) {
                this.AddNode(child.Value, tableSql);
            }
        }

        protected AddNodeResult AddNode(FetchNode node, StringBuilder tableSql, StringBuilder columnSql, bool selectQueryFetchAllProperties, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes) {
            // add this node and then it's children
            // add table sql
            var splitOns = new List<string>();
            var map = this.GetMapForNode(node);

            if (node.IsFetched) {
                splitOns.Add(map.PrimaryKey.Name);
            }

            this.AddTableSqlForNode(node, map, tableSql);

            // add the columns
            if (node.IsFetched) {
                var columns = GetColumnsWithIncludesAndExcludes(includes, excludes, map, selectQueryFetchAllProperties);
                columns = columns.Where(
                    c => !node.Children.ContainsKey(c.Name) || !node.Children[c.Name]
                                                                    .IsFetched);
                foreach (var column in columns) {
                    columnSql.Append(", ");
                    this.AddColumn(columnSql, column, node.Alias);
                }
            }

            // add its children
            var signatureBuilder = new StringBuilder();
            foreach (var child in node.Children) {
                var signature = this.AddNode(child.Value, tableSql, columnSql, selectQueryFetchAllProperties, includes, excludes);
                if (child.Value.IsFetched) {
                    signatureBuilder.Append(signature.Signature);
                    splitOns.AddRange(signature.SplitOn);
                }
            }

            var actualSignature = signatureBuilder.ToString();
            if (node.IsFetched) {
                actualSignature = node.Column.FetchId + "S" + actualSignature + "E";
            }

            return new AddNodeResult {
                                         Signature = actualSignature,
                                         SplitOn = splitOns
                                     };
        }

        private void AddTableSqlForNode(FetchNode node, IMap map, StringBuilder tableSql) {
            // if this is a non-nullable relationship and we've not already done a left join on the way to this node
            // we can do an inner join
            tableSql.Append(
                node.InferredInnerJoin || (!node.Column.IsNullable && node.Column.Relationship != RelationshipType.OneToMany && !this.HasAnyNullableAncestor(node.Parent))
                    ? " inner join "
                    : " left join ");
            this.Dialect.AppendQuotedTableName(tableSql, map);
            tableSql.Append(" as " + node.Alias);

            if (node.Column.Relationship == RelationshipType.ManyToOne || node.Column.Relationship == RelationshipType.OneToOne) {
                tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.DbName + " = " + node.Alias + "." + map.PrimaryKey.DbName);
            }
            else if (node.Column.Relationship == RelationshipType.OneToMany) {
                tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.Map.PrimaryKey.DbName + " = " + node.Alias + "." + node.Column.ChildColumn.DbName);
            }
        }

        private IMap GetMapForNode(FetchNode node) {
            IMap map;
            if (node.Column.Relationship == RelationshipType.OneToMany) {
                map = this.Configuration.GetMap(node.Column.Type.GetGenericArguments()[0]);
            }
            else if (node.Column.Relationship == RelationshipType.ManyToOne || node.Column.Relationship == RelationshipType.OneToOne) {
                map = this.Configuration.GetMap(node.Column.Type);
            }
            else {
                throw new NotSupportedException();
            }

            return map;
        }

        private bool HasAnyNullableAncestor(FetchNode node) {
            if (node.Column == null) {
                return false;
            }

            if (node.Column.IsNullable && !node.InferredInnerJoin) {
                return true;
            }

            return this.HasAnyNullableAncestor(node.Parent);
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

        protected static IEnumerable<IColumn> GetColumnsWithIncludesAndExcludes(IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, IMap map, bool fetchAllProperties) {
            var columns = map.OwnedColumns(fetchAllProperties);
            if (includes != null) {
                IList<IColumn> thisIncludes;
                if (includes.TryGetValue(map.Type, out thisIncludes)) {
                    columns = columns.Union(includes[map.Type]);
                }
            }

            if (excludes != null) {
                IList<IColumn> thisExcludes;
                if (excludes.TryGetValue(map.Type, out thisExcludes)) {
                    columns = columns.Where(c => !thisExcludes.Contains(c));
                }
            }

            return columns;
        }

        internal void AddWhereClause<T>(IList<Expression<Func<T, bool>>> whereClauses, StringBuilder sql, AutoNamingDynamicParameters parameters, ref FetchNode rootNode) {
            var whereClauseWriter = new WhereClauseWriter(this.Dialect, this.Configuration);
            var result = whereClauseWriter.GenerateSql(whereClauses, rootNode, parameters);
            if (result.Sql.Length > 0) {
                sql.Append(result.Sql);
            }

            rootNode = result.FetchTree;
        }

        protected bool AddOrderByClause<T>(Queue<OrderClause<T>> orderClauses, StringBuilder sql, FetchNode rootNode, Func<IColumn, FetchNode, string> aliasRewriter = null, Func<IColumn, FetchNode, string> nameRewriter = null) {
            if (orderClauses.Count == 0) {
                return false;
            }

            sql.Append(" order by ");
            var orderClauseWriter = new OrderClauseWriter(this.Configuration, this.Dialect);
            var containsRootPrimaryKeyClause = false;
            while (orderClauses.Count > 0) {
                var isRootPrimaryKeyClause = false;
                if (aliasRewriter == null && nameRewriter == null) {
                    sql.Append(orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), rootNode, out isRootPrimaryKeyClause));
                }
                else {
                    sql.Append(orderClauseWriter.GetOrderClause(orderClauses.Dequeue(), rootNode, aliasRewriter, nameRewriter, out isRootPrimaryKeyClause));
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

        protected class AddNodeResult {
            public string Signature { get; set; }

            public IList<string> SplitOn { get; set; }
        }
    }
}