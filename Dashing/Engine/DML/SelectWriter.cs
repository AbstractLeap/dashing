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

    internal class SelectWriter : BaseWriter, ISelectWriter {
        public SelectWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) {
            this.fetchTreeParser = new FetchTreeParser(config);
        }

        private static readonly ConcurrentDictionary<WriterQueryCacheKey, string> SingleQueryCache = new ConcurrentDictionary<WriterQueryCacheKey, string>();

        private static readonly ConcurrentDictionary<WriterQueryCacheKey, string> MultipleQueryCache = new ConcurrentDictionary<WriterQueryCacheKey, string>();

        protected FetchTreeParser fetchTreeParser;

        public SqlWriterResult GenerateGetSql<T, TPrimaryKey>(TPrimaryKey id) {
            return new SqlWriterResult(
                SingleQueryCache.GetOrAdd(new WriterQueryCacheKey(this.Configuration, typeof(T)), k => this.GenerateGetSql<T>(false)),
                new DynamicParameters(
                    new {
                            Id = id
                        }));
        }

        public SqlWriterResult GenerateGetSql<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            var primaryKeys = ids as TPrimaryKey[] ?? ids.ToArray();

            if (primaryKeys.Count() == 1) {
                return new SqlWriterResult(
                    SingleQueryCache.GetOrAdd(new WriterQueryCacheKey(this.Configuration, typeof(T)), k => this.GenerateGetSql<T>(false)),
                    new DynamicParameters(
                        new {
                                Id = primaryKeys.Single()
                            }));
            }

            return new SqlWriterResult(
                MultipleQueryCache.GetOrAdd(new WriterQueryCacheKey(this.Configuration, typeof(T)), k => this.GenerateGetSql<T>(true)),
                new DynamicParameters(
                    new {
                            Ids = primaryKeys
                        }));
        }

        private string GenerateGetSql<T>(bool isMultiple) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder("select ");

            foreach (var column in map.OwnedColumns()) {
                this.AddColumn(sql, column);
                sql.Append(", ");
            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append(" from ");
            this.Dialect.AppendQuotedTableName(sql, map);

            sql.Append(" where ");
            sql.Append(map.PrimaryKey.Name);
            sql.Append(
                isMultiple
                    ? " in @Ids"
                    : " = @Id");

            return sql.ToString();
        }

        public SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery, bool enforceAlias = false)
            where T : class, new() {
            // TODO: one StringBuilder to rule them all - Good luck with that ;-) (insertions are expensive)
            var sql = new StringBuilder();
            DynamicParameters parameters = new DynamicParameters();

            // get fetch tree structure
            int aliasCounter;
            int numberCollectionFetches;
            var rootNode = this.fetchTreeParser.GetFetchTree(selectQuery, out aliasCounter, out numberCollectionFetches);

            IDictionary<Type, IList<IColumn>> includes = null;
            IDictionary<Type, IList<IColumn>> excludes = null;
            if ((selectQuery.Includes != null && selectQuery.Includes.Any()) || (selectQuery.Excludes != null && selectQuery.Excludes.Any())) {
                var parser = new IncludeExcludeParser(this.Configuration);
                GetIncludeExcludes<T>(selectQuery.Includes, ref includes, parser);
                GetIncludeExcludes<T>(selectQuery.Excludes, ref excludes, parser);
            }

            if (numberCollectionFetches > 0) {
                if (numberCollectionFetches > 1 && (rootNode.Children.Count(c => c.Value.Column.Relationship == RelationshipType.OneToMany || c.Value.ContainedCollectionfetchesCount > 0) > 1)) {
                    // multiple one to many branches so we'll perform a union query
                    if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                        // TODO this is temporary, should generate union query similar to next
                        rootNode = this.GeneratePagingCollectionSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, includes, excludes, ref parameters);
                    }
                    else {
                        rootNode = this.GenerateNoPagingUnionSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, includes, excludes, ref parameters);
                    }
                }
                else {
                    if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                        // we're sub-selecting so need to use a subquery
                        rootNode = this.GeneratePagingCollectionSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, includes, excludes, ref parameters);
                    }
                    else {
                        // we're fetching all things
                        rootNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, includes, excludes, ref parameters);
                    }
                }
            }
            else {
                // no collection fetches
                rootNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, includes, excludes, ref parameters);
            }

            return new SelectWriterResult(sql.ToString(), parameters, rootNode) {
                                                                                    NumberCollectionsFetched = numberCollectionFetches
                                                                                };
        }

        private static void GetIncludeExcludes<T>(IList<Expression> expressions, ref IDictionary<Type, IList<IColumn>> includes, IncludeExcludeParser parser)
            where T : class, new() {
            if (expressions != null) {
                includes = new Dictionary<Type, IList<IColumn>>();
                foreach (var expression in expressions) {
                    var column = parser.ParseExpression(expression);
                    if (!includes.ContainsKey(column.Map.Type)) {
                        includes.Add(column.Map.Type, new List<IColumn>());
                    }

                    includes[column.Map.Type]
                        .Add(column);
                }
            }
        }

        private FetchNode GenerateNoPagingUnionSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, FetchNode rootNode, StringBuilder sql, int numberCollectionFetches, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, ref DynamicParameters parameters)
            where T : class, new() {
            var numQueries = rootNode.Children.Count(c => c.Value.Column.Relationship == RelationshipType.OneToMany || c.Value.ContainedCollectionfetchesCount > 0);
            var whereSql = new StringBuilder();
            parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            var subQueryColumnSqls = new StringBuilder[numQueries];
            var subQueryTableSqls = new StringBuilder[numQueries];
            for (var i = 0; i < numQueries; i++) {
                subQueryColumnSqls[i] = new StringBuilder();
                subQueryTableSqls[i] = new StringBuilder();
            }

            var outerQueryColumnSql = new StringBuilder();

            // add root columns
            foreach (var column in GetColumnsWithIncludesAndExcludes(includes, excludes, this.Configuration.GetMap<T>(), selectQuery.FetchAllProperties)
                .Where(
                    c => !rootNode.Children.ContainsKey(c.Name) || !rootNode.Children[c.Name]
                                                                            .IsFetched)) {
                foreach (var subQuery in subQueryColumnSqls) {
                    this.AddColumn(subQuery, column, rootNode.Alias, column.DbName + rootNode.Alias);
                    subQuery.Append(", ");
                }

                outerQueryColumnSql.Append("i.");
                this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName + rootNode.Alias);
                outerQueryColumnSql.Append(" as ");
                this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName);
                outerQueryColumnSql.Append(", ");
            }

            // remove extraneous ,
            outerQueryColumnSql.Remove(outerQueryColumnSql.Length - 2, 2);
            foreach (var subQuery in subQueryColumnSqls) {
                subQuery.Remove(subQuery.Length - 2, 2);
            }

            this.AddTablesForNoPagingUnion(selectQuery, outerQueryColumnSql, subQueryColumnSqls, subQueryTableSqls, rootNode, includes, excludes);

            // add order by
            var orderSql = new StringBuilder();
            if (selectQuery.OrderClauses.Any()) {
                var containsPrimaryKeyClause = this.AddOrderByClause(selectQuery.OrderClauses, orderSql, rootNode, (c, n) => "i", (c, n) => c.DbName + n.Alias);
                if (!containsPrimaryKeyClause) {
                    this.AppendDefaultOrderBy<T>(
                        rootNode,
                        orderSql,
                        "i",
                        this.Configuration.GetMap<T>()
                            .PrimaryKey.DbName + rootNode.Alias,
                        false);
                }
            }
            else {
                this.AppendDefaultOrderBy<T>(
                    rootNode,
                    orderSql,
                    "i",
                    this.Configuration.GetMap<T>()
                        .PrimaryKey.DbName + rootNode.Alias);
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
            return rootNode;
        }

        private FetchNode GeneratePagingCollectionSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, FetchNode rootNode, StringBuilder sql, int numberCollectionFetches, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, ref DynamicParameters parameters)
            where T : class, new() {
            // we write a subquery for the root type and all Many-to-One coming off it, we apply paging to that
            // we then left join to all of the collection columns
            // we need to apply the order by outside of the join as well
            var whereSql = new StringBuilder();
            parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            // add root columns
            var innerColumnSql = new StringBuilder();
            this.AddRootColumns(selectQuery, innerColumnSql, rootNode, includes, excludes);
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
            this.AddTablesForPagedCollection(selectQuery, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, rootNode, includes, excludes);

            // add order by
            var innerOrderSql = new StringBuilder();
            var orderClauses = new Queue<OrderClause<T>>(selectQuery.OrderClauses); // clone the queue for use in the outer clause
            if (selectQuery.OrderClauses.Any()) {
                this.AddOrderByClause(selectQuery.OrderClauses, innerOrderSql, rootNode);
            }
            else {
                this.AppendDefaultOrderBy<T>(rootNode, innerOrderSql);
            }

            // construct the query
            var innerSql = new StringBuilder("select ");
            innerSql.Append(innerColumnSql)
                    .Append(innerTableSql)
                    .Append(whereSql)
                    .Append(innerOrderSql);
            //// if anything is added after orderSql then the paging will probably need changing

            // apply paging to inner query
            if (parameters == null) {
                parameters = new DynamicParameters();
            }

            this.Dialect.ApplySkipTake(innerSql, innerOrderSql, selectQuery.TakeN, selectQuery.SkipN);
            if (selectQuery.TakeN > 0) {
                parameters.Add("@take", selectQuery.TakeN);
            }

            if (selectQuery.SkipN > 0) {
                parameters.Add("@skip", selectQuery.SkipN);
            }

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateOnQueryFinish(innerSql);
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
                var containsPrimaryKeyClause = this.AddOrderByClause(orderClauses, outerOrderSql, rootNode, (c, n) => "i", (c, n) => c.DbName);
                if (!containsPrimaryKeyClause) {
                    this.AppendDefaultOrderBy<T>(rootNode, outerOrderSql, "i", isFirstOrderClause: false);
                }
            }
            else {
                this.AppendDefaultOrderBy<T>(rootNode, outerOrderSql, "i");
            }

            sql.Append(outerOrderSql);
            return rootNode;
        }

        private FetchNode GenerateNoPagingSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, FetchNode rootNode, StringBuilder sql, int numberCollectionFetches, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, ref DynamicParameters parameters)
            where T : class, new() {
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            var whereSql = new StringBuilder();
            var orderSql = new StringBuilder();
            if (rootNode == null && enforceAlias) {
                rootNode = new FetchNode {
                                             Alias = "t"
                                         };
            }

            // add where clause
            parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            // add select columns
            this.AddRootColumns(selectQuery, columnSql, rootNode, includes, excludes); // do columns second as we may not be fetching but need joins for the where clause

            // add in the tables
            this.AddTables(selectQuery, tableSql, columnSql, rootNode, includes, excludes);

            // add order by
            if (selectQuery.OrderClauses.Any()) {
                var containsPrimaryKeyClause = this.AddOrderByClause(selectQuery.OrderClauses, orderSql, rootNode);
                if (numberCollectionFetches > 0 && !containsPrimaryKeyClause) {
                    this.AppendDefaultOrderBy<T>(rootNode, orderSql, isFirstOrderClause: false);
                }
            }
            else if (numberCollectionFetches > 0 || selectQuery.SkipN > 0 || selectQuery.TakeN > 0) {
                // need to add a default order on the sort clause
                this.AppendDefaultOrderBy<T>(rootNode, orderSql);
            }

            // construct the query
            sql.Append("select ");
            sql.Append(columnSql);
            sql.Append(tableSql);
            sql.Append(whereSql);
            sql.Append(orderSql);
            //// if anything is added after orderSql then the paging will probably need changing

            // apply paging
            // only add paging to the query if it doesn't have any collection fetches
            if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                if (parameters == null) {
                    parameters = new DynamicParameters();
                }

                this.Dialect.ApplySkipTake(sql, orderSql, selectQuery.TakeN, selectQuery.SkipN);
                if (selectQuery.TakeN > 0) {
                    parameters.Add("@take", selectQuery.TakeN);
                }

                if (selectQuery.SkipN > 0) {
                    parameters.Add("@skip", selectQuery.SkipN);
                }
            }

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateOnQueryFinish(sql);
            }

            return rootNode;
        }

        private void AppendDefaultOrderBy<T>(FetchNode rootNode, StringBuilder orderSql, string alias = null, string name = null, bool isFirstOrderClause = true) {
            if (isFirstOrderClause) {
                orderSql.Append(" order by ");
            }
            else {
                orderSql.Append(", ");
            }

            if (rootNode != null) {
                orderSql.Append(alias ?? rootNode.Alias);
                orderSql.Append('.');
            }

            this.Dialect.AppendQuotedName(
                orderSql,
                name ?? this.Configuration.GetMap<T>()
                            .PrimaryKey.DbName);
        }

        protected void AddTables<T>(SelectQuery<T> selectQuery, StringBuilder tableSql, StringBuilder columnSql, FetchNode rootNode, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes)
            where T : class, new() {
            // separate string builder for the tables as we use the sql builder for fetch columns
            tableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Configuration.GetMap<T>());

            if (rootNode != null) {
                tableSql.Append(" as t");
                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(tableSql);
                }

                if (rootNode.Children.Any()) {
                    // now let's go through the tree and generate the sql
                    var signatureBuilder = new StringBuilder();
                    var splitOns = new List<string>();
                    foreach (var node in rootNode.Children) {
                        var signature = this.AddNode(node.Value, tableSql, columnSql, selectQuery.FetchAllProperties, includes, excludes);
                        if (node.Value.IsFetched) {
                            signatureBuilder.Append(signature.Signature);
                            splitOns.AddRange(signature.SplitOn);
                        }
                    }

                    rootNode.FetchSignature = signatureBuilder.ToString();
                    rootNode.SplitOn = string.Join(",", splitOns);
                }
            }
            else {
                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(tableSql);
                }
            }
        }

        private void AddTablesForNoPagingUnion<T>(SelectQuery<T> selectQuery, StringBuilder outerQueryColumnSql, StringBuilder[] subQueryColumnSqls, StringBuilder[] subQueryTableSqls, FetchNode rootNode, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes)
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
            var signatureBuilder = new StringBuilder();
            var splitOns = new List<string>();
            var insideQueryN = 0;
            var hasSeenFirstCollection = false;
            foreach (var node in rootNode.Children) {
                var signature = this.AddNodeForNonPagedUnion(node.Value, outerQueryColumnSql, subQueryColumnSqls, subQueryTableSqls, ref insideQueryN, false, ref hasSeenFirstCollection, includes, excludes, selectQuery.FetchAllProperties);
                if (node.Value.IsFetched) {
                    signatureBuilder.Append(signature.Signature);
                    splitOns.AddRange(signature.SplitOn);
                }
            }

            rootNode.FetchSignature = signatureBuilder.ToString();
            rootNode.SplitOn = string.Join(",", splitOns);
        }

        private void AddTablesForPagedCollection<T>(SelectQuery<T> selectQuery, StringBuilder innerTableSql, StringBuilder outerTableSql, StringBuilder innerColumnSql, StringBuilder outerColumnSql, FetchNode rootNode, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes)
            where T : class, new() {
            innerTableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(innerTableSql, this.Configuration.GetMap<T>());
            innerTableSql.Append(" as t");

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateUsingTableHint(innerTableSql);
            }

            // go through the tree and generate the sql
            var signatureBuilder = new StringBuilder();
            var splitOns = new List<string>();
            foreach (var node in rootNode.Children) {
                var signature = this.AddNodeForPagedCollection(node.Value, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, false, includes, excludes, selectQuery.FetchAllProperties);
                if (node.Value.IsFetched) {
                    signatureBuilder.Append(signature.Signature);
                    splitOns.AddRange(signature.SplitOn);
                }
            }

            rootNode.FetchSignature = signatureBuilder.ToString();
            rootNode.SplitOn = string.Join(",", splitOns);
        }

        private AddNodeResult AddNodeForNonPagedUnion(FetchNode node, StringBuilder outerQueryColumnSql, StringBuilder[] subQueryColumnSqls, StringBuilder[] subQueryTableSqls, ref int insideQueryN, bool insideCollectionBranch, ref bool hasSeenFirstCollection, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, bool selectQueryFetchAllProperties) {
            var splitOns = new List<string>();
            IMap map;
            if (node.Column.Relationship == RelationshipType.OneToMany) {
                map = this.Configuration.GetMap(node.Column.Type.GetGenericArguments()[0]);
            }
            else {
                map = this.Configuration.GetMap(node.Column.Type);
            }

            if (node.IsFetched) {
                splitOns.Add(map.PrimaryKey.Name);
            }

            var isNowInsideCollection = insideCollectionBranch || node.Column.Relationship == RelationshipType.OneToMany;
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
                     .Append(node.Alias);
                AppendPagedUnionJoin(node, query);
            }
            else {
                // add these joins to all queries
                foreach (var subQuery in subQueryTableSqls) {
                    subQuery.Append(" left join ");
                    this.Dialect.AppendQuotedTableName(subQuery, map);
                    subQuery.Append(" as ")
                            .Append(node.Alias);
                    AppendPagedUnionJoin(node, subQuery);
                }
            }

            // add the columns
            if (node.IsFetched) {
                if (isNowInsideCollection) {
                    // add columns to subquery, nulls to others and cols to outer
                    foreach (var column in GetColumnsWithIncludesAndExcludes(includes, excludes, map, selectQueryFetchAllProperties)
                        .Where(
                            c => !node.Children.ContainsKey(c.Name) || !node.Children[c.Name]
                                                                            .IsFetched)) {
                        for (var i = 0; i < subQueryColumnSqls.Length; i++) {
                            var subQuery = subQueryColumnSqls[i];
                            subQuery.Append(", ");
                            if (i == insideQueryN) {
                                this.AddColumn(subQuery, column, node.Alias, column.DbName + node.Alias);
                            }
                            else {
                                subQuery.Append("null as ")
                                        .Append(column.DbName + node.Alias);
                            }
                        }

                        outerQueryColumnSql.Append(", ")
                                           .Append("i.");
                        this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName + node.Alias);
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
                    foreach (var column in GetColumnsWithIncludesAndExcludes(includes, excludes, map, selectQueryFetchAllProperties)
                        .Where(
                            c => !node.Children.ContainsKey(c.Name) || !node.Children[c.Name]
                                                                            .IsFetched)) {
                        for (var i = 0; i < subQueryColumnSqls.Length; i++) {
                            var subQuery = subQueryColumnSqls[i];
                            subQuery.Append(", ");
                            this.AddColumn(subQuery, column, node.Alias, column.DbName + node.Alias);
                        }

                        outerQueryColumnSql.Append(", ")
                                           .Append("i.");
                        this.Dialect.AppendQuotedName(outerQueryColumnSql, column.DbName + node.Alias);
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
            var signatureBuilder = new StringBuilder();
            foreach (var child in node.Children) {
                var signature = this.AddNodeForNonPagedUnion(child.Value, outerQueryColumnSql, subQueryColumnSqls, subQueryTableSqls, ref insideQueryN, isNowInsideCollection, ref hasSeenFirstCollection, includes, excludes, selectQueryFetchAllProperties);
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

        private AddNodeResult AddNodeForPagedCollection(FetchNode node, StringBuilder innerTableSql, StringBuilder outerTableSql, StringBuilder innerColumnSql, StringBuilder outerColumnSql, bool isAlongCollectionBranch, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, bool selectQueryFetchAllProperties) {
            var splitOns = new List<string>();
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

            if (node.IsFetched) {
                splitOns.Add(map.PrimaryKey.Name);
            }

            var isNowAlongCollectionBranch = isAlongCollectionBranch || node.Column.Relationship == RelationshipType.OneToMany;
            if (isNowAlongCollectionBranch) {
                outerTableSql.Append(" left join ");
                this.Dialect.AppendQuotedTableName(outerTableSql, map);
                outerTableSql.Append(" as ")
                             .Append(node.Alias);

                if (node.Column.Relationship == RelationshipType.ManyToOne || node.Column.Relationship == RelationshipType.OneToOne) {
                    outerTableSql.Append(" on ")
                                 .Append(node.Parent.Alias)
                                 .Append(".")
                                 .Append(node.Column.DbName) // is this right?
                                 .Append(" = ")
                                 .Append(node.Alias)
                                 .Append(".")
                                 .Append(map.PrimaryKey.DbName);
                }
                else if (node.Column.Relationship == RelationshipType.OneToMany) {
                    // we have to rename the columns inside the select query
                    if (isAlongCollectionBranch) {
                        outerTableSql.Append(" on ")
                                     .Append(node.Parent.Alias)
                                     .Append(".")
                                     .Append(node.Column.Map.PrimaryKey.DbName)
                                     .Append(" = ")
                                     .Append(node.Alias)
                                     .Append(".")
                                     .Append(node.Column.ChildColumn.DbName);
                    }
                    else {
                        if (node.Parent.Parent == null) {
                            // next to root node
                            outerTableSql.Append(" on ")
                                         .Append("i.")
                                         .Append(node.Column.Map.PrimaryKey.DbName)
                                         .Append(" = ")
                                         .Append(node.Alias)
                                         .Append(".")
                                         .Append(node.Column.ChildColumn.DbName);
                        }
                        else {
                            outerTableSql.Append(" on ")
                                         .Append("i.")
                                         .Append(node.Column.Map.PrimaryKey.DbName)
                                         .Append(node.Parent.Alias)
                                         .Append(" = ")
                                         .Append(node.Alias)
                                         .Append(".")
                                         .Append(node.Column.ChildColumn.DbName);
                        }
                    }
                }
            }
            else {
                innerTableSql.Append(" left join ");
                this.Dialect.AppendQuotedTableName(innerTableSql, map);
                innerTableSql.Append(" as ")
                             .Append(node.Alias);
                innerTableSql.Append(" on ")
                             .Append(node.Parent.Alias)
                             .Append(".")
                             .Append(node.Column.DbName)
                             .Append(" = ")
                             .Append(node.Alias)
                             .Append(".")
                             .Append(map.PrimaryKey.DbName);
            }

            // add the columns
            if (node.IsFetched) {
                var columns = GetColumnsWithIncludesAndExcludes(includes, excludes, map, selectQueryFetchAllProperties);
                foreach (var column in columns.Where(
                    c => !node.Children.ContainsKey(c.Name) || !node.Children[c.Name]
                                                                    .IsFetched)) {
                    if (isNowAlongCollectionBranch) {
                        outerColumnSql.Append(", ");
                        this.AddColumn(outerColumnSql, column, node.Alias);
                    }
                    else {
                        innerColumnSql.Append(", ");
                        this.AddColumn(innerColumnSql, column, node.Alias, column.Name + node.Alias);
                        outerColumnSql.Append(", ")
                                      .Append("i.")
                                      .Append(column.Name)
                                      .Append(node.Alias)
                                      .Append(" as ");
                        this.Dialect.AppendQuotedName(outerColumnSql, column.DbName);
                    }
                }
            }

            // add its children
            var signatureBuilder = new StringBuilder();
            foreach (var child in node.Children) {
                var signature = this.AddNodeForPagedCollection(child.Value, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, isNowAlongCollectionBranch, includes, excludes, selectQueryFetchAllProperties);
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

        private bool HasAnyNullableAncestor(FetchNode node) {
            if (node.Column == null) {
                return false;
            }

            if (node.Column.IsNullable) {
                return true;
            }

            return this.HasAnyNullableAncestor(node.Parent);
        }

        private AddNodeResult AddNode(FetchNode node, StringBuilder tableSql, StringBuilder columnSql, bool selectQueryFetchAllProperties, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes) {
            // add this node and then it's children
            // add table sql
            var splitOns = new List<string>();
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

            if (node.IsFetched) {
                splitOns.Add(map.PrimaryKey.Name);
            }

            // if this is a non-nullable relationship and we've not already done a left join on the way to this node
            // we can do an inner join
            tableSql.Append(node.Column.IsNullable || node.Column.Relationship == RelationshipType.OneToMany || this.HasAnyNullableAncestor(node.Parent)
                                ? " left join " 
                                : " inner join ");
            this.Dialect.AppendQuotedTableName(tableSql, map);
            tableSql.Append(" as " + node.Alias);

            if (node.Column.Relationship == RelationshipType.ManyToOne || node.Column.Relationship == RelationshipType.OneToOne) {
                tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.DbName + " = " + node.Alias + "." + map.PrimaryKey.DbName);
            }
            else if (node.Column.Relationship == RelationshipType.OneToMany) {
                tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.Map.PrimaryKey.DbName + " = " + node.Alias + "." + node.Column.ChildColumn.DbName);
            }

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

        private void AddRootColumns<T>(SelectQuery<T> selectQuery, StringBuilder columnSql, FetchNode rootNode, IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, bool removeTrailingComma = true)
            where T : class, new() {
            var alias = rootNode != null
                            ? rootNode.Alias
                            : null;

            if (selectQuery.Projection == null) {
                var columns = GetColumnsWithIncludesAndExcludes(includes, excludes, this.Configuration.GetMap<T>(), selectQuery.FetchAllProperties);
                columns = columns.Where(
                    c => rootNode == null || !rootNode.Children.ContainsKey(c.Name) || !rootNode.Children[c.Name]
                                                                                                .IsFetched);
                foreach (var column in columns) {
                    this.AddColumn(columnSql, column, alias);
                    columnSql.Append(", ");
                }
            }

            if (removeTrailingComma) {
                columnSql.Remove(columnSql.Length - 2, 2);
            }
        }

        private static IEnumerable<IColumn> GetColumnsWithIncludesAndExcludes(IDictionary<Type, IList<IColumn>> includes, IDictionary<Type, IList<IColumn>> excludes, IMap map, bool fetchAllProperties) {
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

        private void AddColumn(StringBuilder sql, IColumn column, string tableAlias = null, string columnAlias = null) {
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

        private static void AppendPagedUnionJoin(FetchNode node, StringBuilder subQuery) {
            if (node.Column.Relationship == RelationshipType.ManyToOne) {
                subQuery.Append(" on ")
                        .Append(node.Parent.Alias)
                        .Append(".")
                        .Append(node.Column.DbName)
                        .Append(" = ")
                        .Append(node.Alias)
                        .Append(".")
                        .Append(node.Column.ParentMap.PrimaryKey.DbName);
            }
            else if (node.Column.Relationship == RelationshipType.OneToOne) {
                subQuery.Append(" on ")
                        .Append(node.Parent.Alias)
                        .Append(".")
                        .Append(node.Column.Map.PrimaryKey.DbName)
                        .Append(" = ")
                        .Append(node.Alias)
                        .Append(".")
                        .Append(node.Column.OppositeColumn.DbName);
            }
            else {
                subQuery.Append(" on ")
                        .Append(node.Parent.Alias)
                        .Append(".")
                        .Append(node.Column.Map.PrimaryKey.DbName)
                        .Append(" = ")
                        .Append(node.Alias)
                        .Append(".")
                        .Append(node.Column.ChildColumn.DbName);
            }
        }

        private class AddNodeResult {
            public string Signature { get; set; }

            public IList<string> SplitOn { get; set; }
        }
    }
}