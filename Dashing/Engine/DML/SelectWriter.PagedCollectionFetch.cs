namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Extensions;

    internal partial class SelectWriter {
        private QueryTree GeneratePagingCollectionSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, QueryTree rootQueryNode, StringBuilder sql, int numberCollectionFetches, AutoNamingDynamicParameters parameters, IAliasProvider aliasProvider, bool isProjectedQuery)
            where T : class, new() {
            // we write a subquery for the root type and all Many-to-One coming off it, we apply paging to that
            // we then left join to all of the collection columns
            // we need to apply the order by outside of the join as well
            var whereSql = new StringBuilder();
            this.AddWhereClause(selectQuery.WhereClauses, whereSql, parameters, aliasProvider, ref rootQueryNode);

            // add root columns
            var innerColumnSql = new StringBuilder();
            this.AddRootColumns(selectQuery, innerColumnSql, rootQueryNode, aliasProvider);
            var innerColLength = innerColumnSql.Length;
            var innerColLengthMinusOne = innerColLength - 1;
            var outerColumnSqlTemp = new char[innerColLength];
            for (var i = 0; i < innerColLength; i++) {
                if (innerColumnSql[i] == 't' && i < innerColLengthMinusOne && (i == 0 || innerColumnSql[i - 1] == ' ') && (innerColumnSql[i + 1] == '.' || innerColumnSql[i + 1] == '_')) {
                    outerColumnSqlTemp[i] = 'i';
                }
                else {
                    outerColumnSqlTemp[i] = innerColumnSql[i];
                }
            }

            var outerColumnSql = new StringBuilder(new string(outerColumnSqlTemp)); // outer columns are the same but reference subquery aliased as i

            var innerTableSql = new StringBuilder();
            var outerTableSql = new StringBuilder();
            this.AddTablesForPagedCollection(selectQuery, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, rootQueryNode, aliasProvider, isProjectedQuery);

            // add order by
            var innerOrderSql = new StringBuilder();
            var orderClauses = new Queue<OrderClause<T>>(selectQuery.OrderClauses); // clone the queue for use in the outer clause
            if (selectQuery.OrderClauses.Any()) {
                this.AddOrderByClause(selectQuery.OrderClauses, innerOrderSql, rootQueryNode, aliasProvider);
            }
            else {
                this.AppendDefaultOrderBy<T>(rootQueryNode, innerOrderSql, aliasProvider);
            }

            // construct the query
            var innerSql = new StringBuilder("select ");
            innerSql.Append(innerColumnSql)
                    .Append(innerTableSql)
                    .Append(whereSql)
                    .Append(innerOrderSql);
            //// if anything is added after orderSql then the paging will probably need changing

            this.Dialect.ApplySkipTake(innerSql, innerOrderSql, selectQuery.TakeN, selectQuery.SkipN);
            if (selectQuery.TakeN > 0) {
                parameters.Add("@take", selectQuery.TakeN);
            }

            if (selectQuery.SkipN > 0) {
                parameters.Add("@skip", selectQuery.SkipN);
            }

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateOnQueryFinish(innerSql, selectQuery.SkipLocked);
            }

            // now construct the outer query
            sql.Append("select ")
               .Append(outerColumnSql)
               .Append(" from (")
               .Append(innerSql)
               .Append(") as i")
               .Append(outerTableSql);
            var outerOrderSql = new StringBuilder();
            if (orderClauses.Any()) {
                var containsPrimaryKeyClause = this.AddOrderByClause(orderClauses, outerOrderSql, rootQueryNode, aliasProvider, aliasRewriter: (c, n) => "i", nameRewriter: (c, n) => c.DbName);
                if (!containsPrimaryKeyClause) {
                    this.AppendDefaultOrderBy<T>(rootQueryNode, outerOrderSql, aliasProvider, "i", isFirstOrderClause: false);
                }
            }
            else {
                this.AppendDefaultOrderBy<T>(rootQueryNode, outerOrderSql, aliasProvider, "i");
            }

            sql.Append(outerOrderSql);
            return rootQueryNode;
        }

        private void AddTablesForPagedCollection<T>(SelectQuery<T> selectQuery, StringBuilder innerTableSql, StringBuilder outerTableSql, StringBuilder innerColumnSql, StringBuilder outerColumnSql, QueryTree rootQueryNode, IAliasProvider aliasProvider, bool isProjectedQuery)
            where T : class, new() {
            innerTableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(innerTableSql, this.Configuration.GetMap<T>());
            innerTableSql.Append(" as t");

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateUsingTableHint(innerTableSql, selectQuery.SkipLocked);
            }

            // go through the tree and generate the sql
            foreach (var node in rootQueryNode.Children) {
                this.AddNodeForPagedCollection(node.Value, aliasProvider, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, false, selectQuery.FetchAllProperties, isProjectedQuery);
            }
        }

        private void AddNodeForPagedCollection(QueryNode queryNode, IAliasProvider aliasProvider, StringBuilder innerTableSql, StringBuilder outerTableSql, StringBuilder innerColumnSql, StringBuilder outerColumnSql, bool isAlongCollectionBranch, bool selectQueryFetchAllProperties, bool isProjectedQuery) {
            IMap map;
            if (queryNode.Column.Relationship == RelationshipType.OneToMany) {
                map = this.Configuration.GetMap(queryNode.Column.Type.GetGenericArguments()[0]);
            }
            else if (queryNode.Column.Relationship == RelationshipType.ManyToOne || queryNode.Column.Relationship == RelationshipType.OneToOne) {
                map = this.Configuration.GetMap(queryNode.Column.Type);
            }
            else {
                throw new NotSupportedException();
            }

            var isNowAlongCollectionBranch = isAlongCollectionBranch || queryNode.Column.Relationship == RelationshipType.OneToMany;
            if (isNowAlongCollectionBranch) {
                outerTableSql.Append(" left join ");
                this.Dialect.AppendQuotedTableName(outerTableSql, map);
                outerTableSql.Append(" as ")
                             .Append(aliasProvider.GetAlias(queryNode));

                if (queryNode.Column.Relationship == RelationshipType.ManyToOne || queryNode.Column.Relationship == RelationshipType.OneToOne) {
                    outerTableSql.Append(" on ")
                                 .Append(aliasProvider.GetAlias(queryNode.Parent))
                                 .Append(".")
                                 .Append(queryNode.Column.DbName) // is this right?
                                 .Append(" = ")
                                 .Append(aliasProvider.GetAlias(queryNode))
                                 .Append(".")
                                 .Append(map.PrimaryKey.DbName);
                }
                else if (queryNode.Column.Relationship == RelationshipType.OneToMany) {
                    // we have to rename the columns inside the select query
                    if (isAlongCollectionBranch) {
                        outerTableSql.Append(" on ")
                                     .Append(aliasProvider.GetAlias(queryNode.Parent))
                                     .Append(".")
                                     .Append(queryNode.Column.Map.PrimaryKey.DbName)
                                     .Append(" = ")
                                     .Append(aliasProvider.GetAlias(queryNode))
                                     .Append(".")
                                     .Append(queryNode.Column.ChildColumn.DbName);
                    }
                    else {
                        if (queryNode.Parent is QueryTree) {
                            // next to root queryNode
                            outerTableSql.Append(" on ")
                                         .Append("i.")
                                         .Append(queryNode.Column.Map.PrimaryKey.DbName)
                                         .Append(" = ")
                                         .Append(aliasProvider.GetAlias(queryNode))
                                         .Append(".")
                                         .Append(queryNode.Column.ChildColumn.DbName);
                        }
                        else {
                            outerTableSql.Append(" on ")
                                         .Append("i.")
                                         .Append(queryNode.Column.Map.PrimaryKey.DbName)
                                         .Append(aliasProvider.GetAlias(queryNode.Parent))
                                         .Append(" = ")
                                         .Append(aliasProvider.GetAlias(queryNode))
                                         .Append(".")
                                         .Append(queryNode.Column.ChildColumn.DbName);
                        }
                    }
                }
            }
            else {
                innerTableSql.Append(" left join ");
                this.Dialect.AppendQuotedTableName(innerTableSql, map);
                innerTableSql.Append(" as ")
                             .Append(aliasProvider.GetAlias(queryNode));
                innerTableSql.Append(" on ")
                             .Append(aliasProvider.GetAlias(queryNode.Parent))
                             .Append(".")
                             .Append(queryNode.Column.DbName)
                             .Append(" = ")
                             .Append(aliasProvider.GetAlias(queryNode))
                             .Append(".")
                             .Append(map.PrimaryKey.DbName);
            }

            // add the columns
            if (queryNode.IsFetched) {
                var columns = queryNode.GetSelectedColumns();
                foreach (var columnEntry in columns.AsSmartEnumerable()) {
                    var column = columnEntry.Value;
                    if (isNowAlongCollectionBranch) {
                        outerColumnSql.Append(", ");
                        this.AddColumn(outerColumnSql, column, aliasProvider.GetAlias(queryNode));
                    }
                    else {
                        innerColumnSql.Append(", ");
                        this.AddColumn(innerColumnSql, column, aliasProvider.GetAlias(queryNode), column.Name + aliasProvider.GetAlias(queryNode));
                        outerColumnSql.Append(", ")
                                      .Append("i.")
                                      .Append(column.Name)
                                      .Append(aliasProvider.GetAlias(queryNode))
                                      .Append(" as ");
                        this.Dialect.AppendQuotedName(outerColumnSql, column.DbName);
                    }
                }
            }

            // add its children
            foreach (var child in queryNode.Children) {
                this.AddNodeForPagedCollection(child.Value, aliasProvider, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, isNowAlongCollectionBranch, selectQueryFetchAllProperties, isProjectedQuery);
            }
        }
    }
}