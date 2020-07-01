namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;

    internal partial class SelectWriter : BaseWriter, ISelectWriter {
        public SelectWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) {
            this.fetchTreeParser = new FetchTreeParser(config);
        }

        protected FetchTreeParser fetchTreeParser;

        public SelectWriterResult GenerateSql<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> projectedSelectQuery)
            where TBase : class, new() {
            // get fetch tree structure
            var rootNode = this.fetchTreeParser.GetFetchTree(projectedSelectQuery.BaseSelectQuery, out _, out var numberCollectionFetches) 
                           ?? new QueryTree(true, projectedSelectQuery.BaseSelectQuery.FetchAllProperties, this.Configuration.GetMap<TBase>());

            // add in the projection struction
            var selectProjectionParser = new SelectProjectionParser<TBase>(this.Configuration);
            selectProjectionParser.ParseExpression(projectedSelectQuery.ProjectionExpression, rootNode);

            return this.InnerGenerateSql(projectedSelectQuery.BaseSelectQuery, new AutoNamingDynamicParameters(), new DefaultAliasProvider(), false, rootNode, numberCollectionFetches, new StringBuilder(), true);
        }

        public SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery, AutoNamingDynamicParameters parameters = null, bool enforceAlias = false)
            where T : class, new() {
            // get fetch tree structure
            var rootNode = this.fetchTreeParser.GetFetchTree(selectQuery, out _, out var numberCollectionFetches)
                ?? new QueryTree(false, selectQuery.FetchAllProperties, this.Configuration.GetMap<T>());

            return this.InnerGenerateSql(selectQuery, parameters ?? new AutoNamingDynamicParameters(), new DefaultAliasProvider(), enforceAlias, rootNode, numberCollectionFetches, new StringBuilder(), false);
        }

        private SelectWriterResult InnerGenerateSql<T>(SelectQuery<T> selectQuery, AutoNamingDynamicParameters parameters, IAliasProvider aliasProvider, bool enforceAlias, QueryTree rootQueryNode, int numberCollectionFetches, StringBuilder sql, bool isProjectedQuery)
            where T : class, new() { // add in any includes or excludes
            if (rootQueryNode is null) {
                throw new ArgumentNullException(nameof(rootQueryNode));
            }

            if ((selectQuery.Includes != null && selectQuery.Includes.Any()) || (selectQuery.Excludes != null && selectQuery.Excludes.Any())) {
                var parser = new IncludeExcludeParser(this.Configuration);
                GetIncludeExcludes<T>(selectQuery.Includes, parser, rootQueryNode, true);
                GetIncludeExcludes<T>(selectQuery.Excludes, parser, rootQueryNode, false);
            }

            if (numberCollectionFetches > 0) {
                if (numberCollectionFetches > 1 && (rootQueryNode.Children.Count(c => c.Value.Column.Relationship == RelationshipType.OneToMany || c.Value.ContainedCollectionFetchesCount > 0) > 1)) {
                    // multiple one to many branches so we'll perform a union query
                    if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                        // TODO this is temporary, should generate union query similar to next
                        rootQueryNode = this.GeneratePagingCollectionSql(selectQuery, enforceAlias, rootQueryNode, sql, numberCollectionFetches, parameters, aliasProvider, isProjectedQuery);
                    }
                    else {
                        rootQueryNode = this.GenerateNoPagingUnionSql(selectQuery, enforceAlias, rootQueryNode, sql, numberCollectionFetches, parameters, aliasProvider, isProjectedQuery);
                    }
                }
                else {
                    if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                        // we're sub-selecting so need to use a subquery
                        rootQueryNode = this.GeneratePagingCollectionSql(selectQuery, enforceAlias, rootQueryNode, sql, numberCollectionFetches, parameters, aliasProvider, isProjectedQuery);
                    }
                    else {
                        // we're fetching all things
                        rootQueryNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, rootQueryNode, sql, numberCollectionFetches, parameters, aliasProvider, isProjectedQuery);
                    }
                }
            }
            else {
                // no collection fetches
                // see if we can transform to union query for non-root disjunctions
                var nonRootDisjunctionTransformationSucceeded = false;
                if (selectQuery.TakeN == 0 && selectQuery.SkipN == 0) {
                    var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(this.Configuration);
                    int substitutedWhereClauseIndex = -1;
                    Expression<Func<T, bool>> substitutedWhereClause = null;
                    IEnumerable<Expression<Func<T, bool>>> substitutions = null;
                    foreach (var whereClauseEntry in selectQuery.WhereClauses.AsSmartEnumerable()) {
                        var whereClause = whereClauseEntry.Value;
                        var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(whereClause);
                        if (result.ContainsOuterJoinDisjunction) {
                            if (substitutedWhereClause != null) {
                                // we'll bail out here as we're not supporting multiple disjunctions
                                substitutedWhereClause = null;
                                break;
                            }

                            substitutedWhereClauseIndex = whereClauseEntry.Index;
                            substitutedWhereClause = whereClause;
                            substitutions = result.UnionWhereClauses;
                        }
                    }

                    if (substitutedWhereClause != null) {
                        // we don't want to order the unioned queries, we'll order them subsequently
                        var originalOrderClauses = selectQuery.OrderClauses;
                        selectQuery.OrderClauses = new Queue<OrderClause<T>>();

                        // we need to copy the fetch queryNode and re-use it inside every query
                        var originalRootNode = rootQueryNode.Clone();

                        foreach (var substitution in substitutions.AsSmartEnumerable()) {
                            // swap out the original where clause for the substitute
                            selectQuery.WhereClauses.RemoveAt(substitutedWhereClauseIndex);
                            selectQuery.WhereClauses.Insert(substitutedWhereClauseIndex, substitution.Value);
                            var substitutionRootNode = originalRootNode.Clone();
                            rootQueryNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, substitutionRootNode, sql, numberCollectionFetches, parameters, new DefaultAliasProvider(), isProjectedQuery);
                            if (!substitution.IsLast) {
                                sql.Append(" union ");
                            }
                        }

                        if (originalOrderClauses != null && originalOrderClauses.Any()) {
                            this.AddOrderByClause(originalOrderClauses, sql, rootQueryNode, aliasProvider);
                        }

                        nonRootDisjunctionTransformationSucceeded = true;
                    }
                }

                if (!nonRootDisjunctionTransformationSucceeded) {
                    rootQueryNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, rootQueryNode, sql, numberCollectionFetches, parameters, aliasProvider, isProjectedQuery);
                }
            }

            return new SelectWriterResult(sql.ToString(), parameters, rootQueryNode) {
                                                                                    NumberCollectionsFetched = numberCollectionFetches
                                                                                };
        }

        private static void GetIncludeExcludes<T>(IList<Expression> expressions, IncludeExcludeParser parser, QueryTree rootQueryNode, bool isInclude /* as opposed to exclude */)
            where T : class, new() {
            if (expressions == null) {
                return;
            }

            foreach (var expression in expressions) {
                parser.ParseExpression<T>(expression, rootQueryNode, isInclude);
            }
        }

        private void AppendDefaultOrderBy<T>(QueryTree rootQueryNode, StringBuilder orderSql, IAliasProvider aliasProvider, string alias = null, string name = null, bool isFirstOrderClause = true) {
            if (isFirstOrderClause) {
                orderSql.Append(" order by ");
            }
            else {
                orderSql.Append(", ");
            }

            if (rootQueryNode != null) {
                orderSql.Append(alias ?? aliasProvider.GetAlias(rootQueryNode));
                orderSql.Append('.');
            }

            this.Dialect.AppendQuotedName(
                orderSql,
                name ?? this.Configuration.GetMap<T>()
                            .PrimaryKey.DbName);
        }

        protected void AddTables<T>(SelectQuery<T> selectQuery, StringBuilder tableSql, StringBuilder columnSql, QueryTree rootQueryNode, IAliasProvider aliasProvider, bool isProjectedQuery)
            where T : class, new() {
            // separate string builder for the tables as we use the sql builder for fetch columns
            tableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Configuration.GetMap<T>());

            if (rootQueryNode != null) {
                tableSql.Append(" as t");
                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(tableSql);
                }

                if (rootQueryNode.Children.Any()) {
                    // now let's go through the tree and generate the sql
                    foreach (var node in rootQueryNode.Children) {
                        this.AddNode(node.Value, tableSql, columnSql, aliasProvider, selectQuery.FetchAllProperties, isProjectedQuery);
                    }
                }
            }
            else {
                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(tableSql);
                }
            }
        }

        private void AddRootColumns<T>(SelectQuery<T> selectQuery, StringBuilder columnSql, QueryTree rootQueryNode, IAliasProvider aliasProvider)
            where T : class, new() {
            var alias = aliasProvider.GetAlias(rootQueryNode);
            var columns = rootQueryNode.GetSelectedColumns();
            columns = columns.Where(
                c => !rootQueryNode.Children.ContainsKey(c.Name) || !rootQueryNode.Children[c.Name].IsFetched);
            foreach (var columnEntry in columns.AsSmartEnumerable()) {
                this.AddColumn(columnSql, columnEntry.Value, alias);
                if (!columnEntry.IsLast) {
                    columnSql.Append(", ");
                }
            }
        }
    }
}