namespace Dashing.Engine.DML {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Extensions;

    internal partial class SelectWriter {
        private QueryTree GenerateNoPagingUnionSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, QueryTree rootQueryNode, StringBuilder sql, int numberCollectionFetches, AutoNamingDynamicParameters parameters, IAliasProvider aliasProvider, bool isProjectedQuery)
            where T : class, new() {
            var numQueries = rootQueryNode.Children.Count(c => c.Value.Column.Relationship == RelationshipType.OneToMany || c.Value.ContainedCollectionFetchesCount > 0);
            var whereSql = new StringBuilder();
            this.AddWhereClause(selectQuery.WhereClauses, whereSql, parameters, aliasProvider, ref rootQueryNode);

            var subQueryColumnSqls = new StringBuilder[numQueries];
            var subQueryTableSqls = new StringBuilder[numQueries];
            for (var i = 0; i < numQueries; i++) {
                subQueryColumnSqls[i] = new StringBuilder();
                subQueryTableSqls[i] = new StringBuilder();
            }

            var outerQueryColumnSql = new StringBuilder();

            // add root columns
            foreach (var column in rootQueryNode.GetSelectedColumns()) {
                foreach (var subQuery in subQueryColumnSqls) {
                    this.AddColumn(subQuery, column, aliasProvider.GetAlias(rootQueryNode), column.DbName + aliasProvider.GetAlias(rootQueryNode));
                    subQuery.Append(", ");
                }

                outerQueryColumnSql.Append("i.");
                this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName + aliasProvider.GetAlias(rootQueryNode));
                outerQueryColumnSql.Append(" as ");
                this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName);
                outerQueryColumnSql.Append(", ");
            }

            // remove extraneous ,
            outerQueryColumnSql.Remove(outerQueryColumnSql.Length - 2, 2);
            foreach (var subQuery in subQueryColumnSqls) {
                subQuery.Remove(subQuery.Length - 2, 2);
            }

            this.AddTablesForNoPagingUnion(selectQuery, aliasProvider, outerQueryColumnSql, subQueryColumnSqls, subQueryTableSqls, rootQueryNode, isProjectedQuery);

            // add order by
            var orderSql = new StringBuilder();
            if (selectQuery.OrderClauses.Any()) {
                var containsPrimaryKeyClause = this.AddOrderByClause(selectQuery.OrderClauses, orderSql, rootQueryNode, aliasProvider, aliasRewriter: (c, n) => "i", nameRewriter: (c, n) => c.DbName + aliasProvider.GetAlias(n));
                if (!containsPrimaryKeyClause) {
                    this.AppendDefaultOrderBy<T>(
                        rootQueryNode,
                        orderSql,
                        aliasProvider,
                        "i",
                        this.Configuration.GetMap<T>()
                            .PrimaryKey.DbName + aliasProvider.GetAlias(rootQueryNode),
                        false);
                }
            }
            else {
                this.AppendDefaultOrderBy<T>(
                    rootQueryNode,
                    orderSql,
                    aliasProvider,
                    "i",
                    this.Configuration.GetMap<T>()
                        .PrimaryKey.DbName + aliasProvider.GetAlias(rootQueryNode));
            }

            // now create the query
            sql.Append("select ")
               .Append(outerQueryColumnSql)
               .Append(" from (");
            for (var i = 0; i < numQueries; i++) {
                sql.Append("select ")
                   .Append(subQueryColumnSqls[i])
                   .Append(subQueryTableSqls[i]);
                if (whereSql.Length > 0) {
                    sql.Append(whereSql);
                }

                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateOnQueryFinish(sql);
                }

                if (i < numQueries - 1) {
                    sql.Append(" union all ");
                }
            }

            sql.Append(") as i");
            sql.Append(orderSql);
            return rootQueryNode;
        }

        private void AddTablesForNoPagingUnion<T>(SelectQuery<T> selectQuery, IAliasProvider aliasProvider, StringBuilder outerQueryColumnSql, StringBuilder[] subQueryColumnSqls, StringBuilder[] subQueryTableSqls, QueryTree rootQueryNode, bool isProjectedQuery)
            where T : class, new() {
            foreach (var subQuery in subQueryTableSqls) {
                subQuery.Append(" from ");
                this.Dialect.AppendQuotedTableName(subQuery, this.Configuration.GetMap<T>());
                subQuery.Append(" as t");

                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(subQuery);
                }
            }

            // go through the tree and generate the sql
            var insideQueryN = 0;
            var hasSeenFirstCollection = false;
            foreach (var node in rootQueryNode.Children) {
                this.AddNodeForNonPagedUnion(node.Value, aliasProvider, outerQueryColumnSql, subQueryColumnSqls, subQueryTableSqls, ref insideQueryN, false, ref hasSeenFirstCollection, selectQuery.FetchAllProperties, isProjectedQuery);
            }
        }

        private void AddNodeForNonPagedUnion(QueryNode queryNode, IAliasProvider aliasProvider, StringBuilder outerQueryColumnSql, StringBuilder[] subQueryColumnSqls, StringBuilder[] subQueryTableSqls, ref int insideQueryN, bool insideCollectionBranch, ref bool hasSeenFirstCollection, bool selectQueryFetchAllProperties, bool isProjectedQuery) {
            IMap map;
            if (queryNode.Column.Relationship == RelationshipType.OneToMany) {
                map = this.Configuration.GetMap(queryNode.Column.Type.GetGenericArguments()[0]);
            }
            else {
                map = this.Configuration.GetMap(queryNode.Column.Type);
            }

            var isNowInsideCollection = insideCollectionBranch || queryNode.Column.Relationship == RelationshipType.OneToMany;
            if (isNowInsideCollection) {
                if (!insideCollectionBranch && hasSeenFirstCollection) {
                    // not inside collection and not first one to many
                    insideQueryN++;
                }

                hasSeenFirstCollection = true;
                StringBuilder query = subQueryTableSqls[insideQueryN];
                query.Append(" left join ");
                this.Dialect.AppendQuotedTableName(query, map);
                query.Append(" as ")
                     .Append(aliasProvider.GetAlias(queryNode));
                AppendPagedUnionJoin(queryNode, aliasProvider, query);
            }
            else {
                // add these joins to all queries
                foreach (var subQuery in subQueryTableSqls) {
                    subQuery.Append(" left join ");
                    this.Dialect.AppendQuotedTableName(subQuery, map);
                    subQuery.Append(" as ")
                            .Append(aliasProvider.GetAlias(queryNode));
                    AppendPagedUnionJoin(queryNode, aliasProvider, subQuery);
                }
            }

            // add the columns
            if (queryNode.IsFetched) {
                if (isNowInsideCollection) {
                    // add columns to subquery, nulls to others and cols to outer
                    foreach (var column in queryNode.GetSelectedColumns()) {
                        for (var i = 0; i < subQueryColumnSqls.Length; i++) {
                            var subQuery = subQueryColumnSqls[i];
                            subQuery.Append(", ");
                            if (i == insideQueryN) {
                                this.AddColumn(subQuery, column, aliasProvider.GetAlias(queryNode), column.DbName + aliasProvider.GetAlias(queryNode));
                            }
                            else {
                                subQuery.Append("null as ")
                                        .Append(column.DbName + aliasProvider.GetAlias(queryNode));
                            }
                        }

                        outerQueryColumnSql.Append(", ")
                                           .Append("i.");
                        this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName + aliasProvider.GetAlias(queryNode));
                        outerQueryColumnSql.Append(" as ");
                        if (column.Relationship == RelationshipType.None) {
                            this.Dialect.AppendQuotedName(outerQueryColumnSql, column.Name);
                        }
                        else {
                            this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName);
                        }
                    }
                }
                else {
                    // add columns to all queries
                    foreach (var columnEntry in queryNode.GetSelectedColumns()
                                                .AsSmartEnumerable()) {
                        var column = columnEntry.Value;

                        for (var i = 0; i < subQueryColumnSqls.Length; i++) {
                            var subQuery = subQueryColumnSqls[i];
                            subQuery.Append(", ");
                            this.AddColumn(subQuery, column, aliasProvider.GetAlias(queryNode), column.DbName + aliasProvider.GetAlias(queryNode));
                        }

                        outerQueryColumnSql.Append(", ")
                                           .Append("i.");
                        this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName + aliasProvider.GetAlias(queryNode));
                        outerQueryColumnSql.Append(" as ");
                        if (column.Relationship == RelationshipType.None) {
                            this.Dialect.AppendQuotedName(outerQueryColumnSql, column.Name);
                        }
                        else {
                            this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName);
                        }
                    }
                }
            }

            // add its children
            foreach (var child in queryNode.Children) {
                this.AddNodeForNonPagedUnion(child.Value, aliasProvider, outerQueryColumnSql, subQueryColumnSqls, subQueryTableSqls, ref insideQueryN, isNowInsideCollection, ref hasSeenFirstCollection, selectQueryFetchAllProperties, isProjectedQuery);
            }
        }

        private static void AppendPagedUnionJoin(QueryNode queryNode, IAliasProvider aliasProvider, StringBuilder subQuery) {
            if (queryNode.Column.Relationship == RelationshipType.ManyToOne) {
                subQuery.Append(" on ")
                        .Append(aliasProvider.GetAlias(queryNode.Parent))
                        .Append(".")
                        .Append(queryNode.Column.DbName)
                        .Append(" = ")
                        .Append(aliasProvider.GetAlias(queryNode))
                        .Append(".")
                        .Append(queryNode.Column.ParentMap.PrimaryKey.DbName);
            }
            else if (queryNode.Column.Relationship == RelationshipType.OneToOne) {
                subQuery.Append(" on ")
                        .Append(aliasProvider.GetAlias(queryNode.Parent))
                        .Append(".")
                        .Append(queryNode.Column.Map.PrimaryKey.DbName)
                        .Append(" = ")
                        .Append(aliasProvider.GetAlias(queryNode))
                        .Append(".")
                        .Append(queryNode.Column.OppositeColumn.DbName);
            }
            else {
                subQuery.Append(" on ")
                        .Append(aliasProvider.GetAlias(queryNode.Parent))
                        .Append(".")
                        .Append(queryNode.Column.Map.PrimaryKey.DbName)
                        .Append(" = ")
                        .Append(aliasProvider.GetAlias(queryNode))
                        .Append(".")
                        .Append(queryNode.Column.ChildColumn.DbName);
            }
        }
    }
}