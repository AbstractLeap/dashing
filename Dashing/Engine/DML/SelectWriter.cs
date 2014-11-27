namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class SelectWriter : BaseWriter, ISelectWriter {
        public SelectWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) {
        }

        private static readonly ConcurrentDictionary<Tuple<Type, string>, string> QueryCache = new ConcurrentDictionary<Tuple<Type, string>, string>();

        public SqlWriterResult GenerateGetSql<T, TPrimaryKey>(TPrimaryKey id) {
            return
                new SqlWriterResult(
                    QueryCache.GetOrAdd(
                        Tuple.Create(typeof(T), "GetSingle"),
                        k => this.GenerateGetSql<T>(false)),
                    new DynamicParameters(new { Id = id }));
        }

        public SqlWriterResult GenerateGetSql<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            var primaryKeys = ids as TPrimaryKey[] ?? ids.ToArray();

            if (primaryKeys.Count() == 1) {
                return new SqlWriterResult(QueryCache.GetOrAdd(Tuple.Create(typeof(T), "GetSingle"), k => this.GenerateGetSql<T>(false)), new DynamicParameters(new { Id = primaryKeys.Single() }));
            }

            return new SqlWriterResult(QueryCache.GetOrAdd(Tuple.Create(typeof(T), "GetMultiple"), k => this.GenerateGetSql<T>(true)), new DynamicParameters(new { Ids = primaryKeys }));
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
            sql.Append(isMultiple ? " in @Ids" : " = @Id");

            return sql.ToString();
        }

        public SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery) {
            // TODO: one StringBuilder to rule them all - Good luck with that ;-) (insertions are expensive)
            var sql = new StringBuilder();
            DynamicParameters parameters = new DynamicParameters();

            // get fetch tree structure
            int aliasCounter;
            int numberCollectionFetches;
            var rootNode = this.GetFetchTree(selectQuery, out aliasCounter, out numberCollectionFetches);

            if (numberCollectionFetches > 1) {
                // we need to write a better query than simply a cross join of everything
                var tableSql = new StringBuilder(" from ");
                this.Dialect.AppendQuotedTableName(tableSql, this.Configuration.GetMap<T>());
                var rootColumnSql = new StringBuilder();
                this.AddColumns(selectQuery, rootColumnSql, rootNode, false);
                var outerColumns = new StringBuilder(rootColumnSql.ToString().Replace("t.", "u."));
                var innerColumnSqls = new List<StringBuilder> { new StringBuilder(rootColumnSql.ToString()) };
                var innerTableSqls = new List<StringBuilder> { new StringBuilder(tableSql.ToString()).Append(" as t") };
                var whereSql = new StringBuilder();

                parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

                var result = this.VisitMultiCollectionTree(rootNode, outerColumns, innerColumnSqls, innerTableSqls);
                rootNode.FetchSignature = result.Signature;
                rootNode.SplitOn = string.Join(",", result.SplitOn);

                // patch it all together
                sql.Append("select ");
                sql.Append(outerColumns.Remove(outerColumns.Length - 2, 2));
                sql.Append(" from (");
                for (var i = 0; i < innerTableSqls.Count; ++i) {
                    sql.Append("select ");
                    sql.Append(innerColumnSqls[i].Remove(innerColumnSqls[i].Length - 2, 2));
                    sql.Append(innerTableSqls[i]);
                    sql.Append(whereSql);
                    sql.Append(" union all ");
                }

                sql.Remove(sql.Length - 11, 11);
                sql.Append(") as u");
            }
            else {
                var columnSql = new StringBuilder();
                var tableSql = new StringBuilder();
                var whereSql = new StringBuilder();
                var orderSql = new StringBuilder();

                // add where clause
                parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

                // add select columns
                this.AddColumns(selectQuery, columnSql, rootNode); // do columns second as we may not be fetching but need joins for the where clause

                // add in the tables
                this.AddTables(selectQuery, tableSql, columnSql, rootNode);

                // add order by
                if (selectQuery.OrderClauses.Any()) {
                    this.AddOrderByClause(selectQuery.OrderClauses, orderSql);
                }
                else if (selectQuery.SkipN > 0) {
                    // need to add a default order on the sort clause
                    orderSql.Append(" order by ");
                    if (rootNode != null) {
                        orderSql.Append(rootNode.Alias);
                        orderSql.Append('.');
                    }

                    this.Dialect.AppendQuotedName(orderSql, this.Configuration.GetMap<T>().PrimaryKey.DbName);
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
                if (numberCollectionFetches == 0 && (selectQuery.TakeN > 0 || selectQuery.SkipN > 0)) {
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
            }

            return new SelectWriterResult(sql.ToString(), parameters, rootNode) { NumberCollectionsFetched = numberCollectionFetches };
        }

        private AddNodeResult VisitMultiCollectionTree(FetchNode node, StringBuilder outerColumns, List<StringBuilder> innerColumnSqls, List<StringBuilder> innerTableSqls) {
            // we walk along the tree creating sub queries as we go
            // if at a node we have a split of collection fetching we'll generate sub queries at that point
            // simple case is current node does not contain any collection fetches
            var splitOns = new List<string>();
            var signatureBuilder = new StringBuilder();
            if (node.ContainedCollectionfetchesCount > 0) {
                // figure out if we have a split here
                int collectionFetchesAtThisLevel = 0;
                int numberOfBranchesWithMultiFetches = 0;
                int currentCountOfSubQueries = innerTableSqls.Count;
                var innerColumnCopy = innerColumnSqls.Last().ToString();
                var innerTableCopy = innerTableSqls.Last().ToString();
                bool splitProcessed = false;
                foreach (var child in node.Children) {
                    if (child.Value.ContainedCollectionfetchesCount > 0) {
                        ++numberOfBranchesWithMultiFetches;
                    }

                    if (child.Value.Column.Relationship == RelationshipType.OneToMany) {
                        ++collectionFetchesAtThisLevel;
                        if (splitProcessed) {
                            innerColumnSqls.Add(new StringBuilder(innerColumnCopy));
                            innerTableSqls.Add(new StringBuilder(innerTableCopy));
                        }
                        else {
                            splitProcessed = true;
                        }
                    }
                }

                var hasSplit = collectionFetchesAtThisLevel > 1 || numberOfBranchesWithMultiFetches > 1;
                if (hasSplit) {
                    // we need to generate a new sub query for the new branch
                    var currentStringBuilderIdx = currentCountOfSubQueries - 1;
                    foreach (var childNode in node.Children) {
                        this.AddNodeSql(outerColumns, innerColumnSqls, innerTableSqls, currentStringBuilderIdx, splitOns, childNode.Value);
                        var result = this.VisitMultiCollectionTree(childNode.Value, outerColumns, innerColumnSqls, innerTableSqls);
                        if (!(childNode.Value.ContainedCollectionfetchesCount == 0 && childNode.Value.Column.Relationship == RelationshipType.ManyToOne)) {
                            currentStringBuilderIdx++;
                        }

                        if (childNode.Value.IsFetched) {
                            signatureBuilder.Append(childNode.Value.Column.FetchId + "S" + result.Signature + "E");
                            splitOns.AddRange(result.SplitOn);
                        }
                    }

                    return new AddNodeResult { Signature = signatureBuilder.ToString(), SplitOn = splitOns };
                }
            }

            // simply add null to the other inner queries and add column names to these queries
            foreach (var child in node.Children) {
                var childNode = child.Value;
                this.AddNodeSql(outerColumns, innerColumnSqls, innerTableSqls, innerTableSqls.Count - 1, splitOns, childNode);
                
                var childResult = this.VisitMultiCollectionTree(childNode, outerColumns, innerColumnSqls, innerTableSqls);
                if (childNode.IsFetched) {
                    signatureBuilder.Append(childNode.Column.FetchId + "S" + childResult.Signature + "E");
                    splitOns.AddRange(childResult.SplitOn);
                }
            }

            return new AddNodeResult { SplitOn = splitOns, Signature = signatureBuilder.ToString() };
        }

        private void AddNodeSql(StringBuilder outerColumns, List<StringBuilder> innerColumnSqls, List<StringBuilder> innerTableSqls, int currentInnerSqlBuilderIndex, List<string> splitOns, FetchNode childNode) {
            IMap map;
            var innerTableSqlBuilder = innerTableSqls.ElementAt(currentInnerSqlBuilderIndex);
            var innerColumnSqlBuilder = innerColumnSqls.ElementAt(currentInnerSqlBuilderIndex);
            if (childNode.Column.Relationship == RelationshipType.OneToMany) {
                map = childNode.Column.ChildColumn.Map;
            }
            else {
                map = childNode.Column.ParentMap;
            }

            if (childNode.IsFetched) {
                splitOns.Add(map.PrimaryKey.DbName);
            }

            innerTableSqlBuilder.Append(" left join ");
            this.Dialect.AppendQuotedTableName(innerTableSqlBuilder, map);
            innerTableSqlBuilder.Append(" as " + childNode.Alias);

            if (childNode.Column.Relationship == RelationshipType.OneToMany) {
                innerTableSqlBuilder.Append(" on " + childNode.Parent.Alias + "." + childNode.Column.Map.PrimaryKey.DbName + " = " + childNode.Alias + "." + childNode.Column.ChildColumn.DbName);
            }
            else {
                innerTableSqlBuilder.Append(" on " + childNode.Parent.Alias + "." + childNode.Column.DbName + " = " + childNode.Alias + "." + map.PrimaryKey.DbName);
            }

            // add the columns
            if (childNode.IsFetched) {
                foreach (var column in map.OwnedColumns().Where(c => !childNode.Children.ContainsKey(c.Name))) {
                    foreach (var sqlBuilder in innerColumnSqls) {
                        if (sqlBuilder == innerColumnSqlBuilder) {
                            // add actual values to last query
                            sqlBuilder.Append(childNode.Alias).Append(".");
                            this.Dialect.AppendQuotedName(sqlBuilder, column.DbName);
                            if (column.Relationship == RelationshipType.ManyToOne) {
                                sqlBuilder.Append(" as ").Append(childNode.Alias).Append("_").Append(column.DbName);
                            }
                            else {
                                sqlBuilder.Append(" as ").Append(childNode.Alias).Append("_").Append(column.Name);
                            }

                            sqlBuilder.Append(", ");
                        }
                        else {
                            // add nulls to other queries
                            if (column.Relationship == RelationshipType.ManyToOne) {
                                sqlBuilder.Append("null as " + childNode.Alias + "_" + column.DbName + ", ");
                            }
                            else {
                                sqlBuilder.Append("null as " + childNode.Alias + "_" + column.Name + ", ");
                            }
                        }
                    }

                    // add columns to outer query
                    if (column.Relationship == RelationshipType.ManyToOne) {
                        outerColumns.Append("u." + childNode.Alias + "_" + column.DbName).Append(" as ").Append(column.DbName).Append(", ");
                    }
                    else {
                        outerColumns.Append("u." + childNode.Alias + "_" + column.Name).Append(" as ").Append(column.Name).Append(", ");
                    }
                }
            }
        }

        protected FetchNode GetFetchTree<T>(SelectQuery<T> selectQuery, out int aliasCounter, out int numberCollectionFetches) {
            FetchNode rootNode = null;
            numberCollectionFetches = 0;
            aliasCounter = 0;

            if (selectQuery.HasFetches()) {
                // now we go through the fetches and generate the tree structure
                rootNode = new FetchNode { Alias = "t" };
                foreach (var fetch in selectQuery.Fetches) {
                    var lambda = fetch as LambdaExpression;
                    if (lambda != null) {
                        var expr = lambda.Body as MemberExpression;
                        var currentNode = rootNode;
                        var entityNames = new Stack<string>();

                        // TODO Change this so that algorithm only goes through tree once
                        // We go through the fetch expression (backwards)
                        while (expr != null) {
                            entityNames.Push(expr.Member.Name);
                            expr = expr.Expression as MemberExpression;
                        }

                        // Now go through the expression forwards adding in nodes where needed
                        this.AddPropertiesToFetchTree<T>(ref aliasCounter, ref numberCollectionFetches, entityNames, currentNode, rootNode);
                    }
                }

                // now iterate through the collection fetches
                foreach (var collectionFetch in selectQuery.CollectionFetches) {
                    var entityNames = new Stack<string>();
                    var currentNode = rootNode;

                    // start at the top of the stack, pop the expression off and do as above
                    while (collectionFetch.Value.Count > 0) {
                        var lambdaExpr = collectionFetch.Value.Pop() as LambdaExpression;
                        if (lambdaExpr != null) {
                            var expr = lambdaExpr.Body as MemberExpression;
                            while (expr != null) {
                                entityNames.Push(expr.Member.Name);
                                expr = expr.Expression as MemberExpression;
                            }
                        }
                    }

                    // add in the initial fetch many
                    var fetchManyLambda = collectionFetch.Key as LambdaExpression;
                    if (fetchManyLambda != null) {
                        var expr = fetchManyLambda.Body as MemberExpression;
                        while (expr != null) {
                            entityNames.Push(expr.Member.Name);
                            expr = expr.Expression as MemberExpression;
                        }
                    }

                    this.AddPropertiesToFetchTree<T>(
                        ref aliasCounter,
                        ref numberCollectionFetches,
                        entityNames,
                        currentNode,
                        rootNode);
                }
            }

            return rootNode;
        }

        private void AddPropertiesToFetchTree<T>(
            ref int aliasCounter,
            ref int numberCollectionFetches,
            Stack<string> entityNames,
            FetchNode currentNode,
            FetchNode rootNode) {
            while (entityNames.Count > 0) {
                var propName = entityNames.Pop();

                // don't add duplicates
                if (!currentNode.Children.ContainsKey(propName)) {
                    var column =
                        this.Configuration.GetMap(
                        currentNode == rootNode ? typeof(T) : (currentNode.Column.Relationship == RelationshipType.OneToMany ? currentNode.Column.Type.GetGenericArguments().First() : currentNode.Column.Type)).Columns[propName];
                    if (column.IsIgnored) {
                        //TODO we should probably warn at this point
                        continue;
                    }

                    if (column.Relationship == RelationshipType.OneToMany) {
                        ++numberCollectionFetches;
                    }

                    // add to tree
                    var node = new FetchNode {
                        Parent = currentNode,
                        Column = column,
                        Alias = "t_" + ++aliasCounter,
                        IsFetched = true
                    };
                    if (column.Relationship == RelationshipType.OneToMany) {
                        // go through and increase the number of contained collections in each parent node
                        var parent = node.Parent;
                        while (parent != null) {
                            ++parent.ContainedCollectionfetchesCount;
                            parent = parent.Parent;
                        }
                    }

                    // insert the node in the correct order (i.e. respecting the FetchId and then all other things that depend on this
                    // should be constant
                    if (currentNode.Children.Any()) {
                        var i = 0;
                        var inserted = false;
                        foreach (var child in currentNode.Children) {
                            if (child.Value.Column.FetchId > column.FetchId) {
                                currentNode.Children.Insert(i, new KeyValuePair<string, FetchNode>(propName, node));
                                inserted = true;
                                break;
                            }

                            i++;
                        }

                        if (!inserted) {
                            currentNode.Children.Add(propName, node);
                        }
                    }
                    else {
                        currentNode.Children.Add(propName, node);
                    }

                    currentNode = node;
                }
                else {
                    currentNode = currentNode.Children[propName];
                }
            }
        }

        protected void AddTables<T>(SelectQuery<T> selectQuery, StringBuilder tableSql, StringBuilder columnSql, FetchNode rootNode) {
            // separate string builder for the tables as we use the sql builder for fetch columns
            tableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Configuration.GetMap<T>());

            if (rootNode != null && rootNode.Children.Any()) {
                tableSql.Append(" as t");
                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(tableSql);
                }

                // now let's go through the tree and generate the sql
                var signatureBuilder = new StringBuilder();
                var splitOns = new List<string>();
                foreach (var node in rootNode.Children) {
                    var signature = this.AddNode(node.Value, tableSql, columnSql);
                    if (node.Value.IsFetched) {
                        signatureBuilder.Append(signature.Signature);
                        splitOns.AddRange(signature.SplitOn);
                    }
                }

                rootNode.FetchSignature = signatureBuilder.ToString();
                rootNode.SplitOn = string.Join(",", splitOns);
            }
            else {
                if (selectQuery.IsForUpdate) {
                    this.Dialect.AppendForUpdateUsingTableHint(tableSql);
                }
            }
        }

        private AddNodeResult AddNode(FetchNode node, StringBuilder tableSql, StringBuilder columnSql) {
            // add this node and then it's children
            // add table sql
            var splitOns = new List<string>();
            IMap map;
            if (node.Column.Relationship == RelationshipType.OneToMany) {
                map = this.Configuration.GetMap(node.Column.Type.GetGenericArguments()[0]);
            }
            else if (node.Column.Relationship == RelationshipType.ManyToOne) {
                map = this.Configuration.GetMap(node.Column.Type);
            }
            else {
                throw new NotSupportedException();
            }

            if (node.IsFetched) {
                splitOns.Add(map.PrimaryKey.Name);
            }

            tableSql.Append(" left join ");
            this.Dialect.AppendQuotedTableName(tableSql, map);
            tableSql.Append(" as " + node.Alias);

            if (node.Column.Relationship == RelationshipType.ManyToOne) {
                tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.DbName + " = " + node.Alias + "." + map.PrimaryKey.DbName);
            }
            else if (node.Column.Relationship == RelationshipType.OneToMany) {
                tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.Map.PrimaryKey.DbName + " = " + node.Alias + "." + node.Column.ChildColumn.DbName);
            }

            // add the columns
            if (node.IsFetched) {
                foreach (var column in map.OwnedColumns().Where(c => !node.Children.ContainsKey(c.Name))) {
                    columnSql.Append(", ");
                    this.AddColumn(columnSql, column, node.Alias);
                }
            }

            // add its children
            var signatureBuilder = new StringBuilder();
            foreach (var child in node.Children) {
                var signature = this.AddNode(child.Value, tableSql, columnSql);
                if (child.Value.IsFetched) {
                    signatureBuilder.Append(signature.Signature);
                    splitOns.AddRange(signature.SplitOn);
                }
            }

            var actualSignature = signatureBuilder.ToString();
            if (node.IsFetched) {
                actualSignature = node.Column.FetchId + "S" + actualSignature + "E";
            }
            
            return new AddNodeResult { Signature = actualSignature, SplitOn = splitOns };
        }

        private void AddColumns<T>(SelectQuery<T> selectQuery, StringBuilder columnSql, FetchNode rootNode, bool removeTrailingComma = true) {
            var alias = rootNode != null ? rootNode.Alias : null;

            if (selectQuery.Projection == null) {
                foreach (var column in this.Configuration.GetMap<T>().OwnedColumns(selectQuery.FetchAllProperties).Where(c => rootNode == null || !rootNode.Children.ContainsKey(c.Name) || !rootNode.Children[c.Name].IsFetched)) {
                    this.AddColumn(columnSql, column, alias);
                    columnSql.Append(", ");
                }
            }

            if (removeTrailingComma) {
                columnSql.Remove(columnSql.Length - 2, 2);
            }
        }

        private void AddColumn(StringBuilder sql, IColumn column, string tableAlias = null) {
            // add the table alias
            if (tableAlias != null) {
                sql.Append(tableAlias + ".");
            }

            // add the column name
            this.Dialect.AppendQuotedName(sql, column.DbName);

            // add a column alias if required
            if (column.DbName != column.Name && column.Relationship == RelationshipType.None) {
                sql.Append(" as " + column.Name);
            }
        }

        private class AddNodeResult {
            public string Signature { get; set; }

            public IList<string> SplitOn { get; set; }
        }
    }
}