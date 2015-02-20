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

        public SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery, bool enforceAlias = false) {
            // TODO: one StringBuilder to rule them all - Good luck with that ;-) (insertions are expensive)
            var sql = new StringBuilder();
            DynamicParameters parameters = new DynamicParameters();

            // get fetch tree structure
            int aliasCounter;
            int numberCollectionFetches;
            var rootNode = this.GetFetchTree(selectQuery, out aliasCounter, out numberCollectionFetches);

            if (numberCollectionFetches > 0) {
                if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                    // we're sub-selecting so need to use a subquery
                    rootNode = this.GeneratePagingCollectionSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, ref parameters);
                }
                else {
                    // we're fetching all things
                    rootNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, ref parameters);
                }
            }
            else {
                // no collection fetches
                rootNode = this.GenerateNoPagingSql(selectQuery, enforceAlias, rootNode, sql, numberCollectionFetches, ref parameters);
            }

            return new SelectWriterResult(sql.ToString(), parameters, rootNode) { NumberCollectionsFetched = numberCollectionFetches };
        }

        private FetchNode GeneratePagingCollectionSql<T>(SelectQuery<T> selectQuery, bool enforceAlias, FetchNode rootNode, StringBuilder sql, int numberCollectionFetches, ref DynamicParameters parameters) {
            // we write a subquery for the root type and all Many-to-One coming off it, we apply paging to that
            // we then left join to all of the collection columns
            // we need to apply the order by outside of the join as well
            var whereSql = new StringBuilder();
            parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            // add root columns
            var innerColumnSql = new StringBuilder();
            this.AddRootColumns(selectQuery, innerColumnSql, rootNode);
            var innerColLength = innerColumnSql.Length;
            var innerColLengthMinusOne = innerColLength - 1;
            var outerColumnSqlTemp = new char[innerColLength];
            for (var i = 0; i < innerColLength; i++) {
                if (innerColumnSql[i] == 't' && i < innerColLengthMinusOne && (i == 0 || innerColumnSql[i - 1] == ' ')
                    && (innerColumnSql[i + 1] == '.' || innerColumnSql[i + 1] == '_')) {
                    outerColumnSqlTemp[i] = 'i';
                }
                else {
                    outerColumnSqlTemp[i] = innerColumnSql[i];
                }
            }

            var outerColumnSql = new StringBuilder(new string(outerColumnSqlTemp)); // outer columns are the same but reference subquery aliased as i

            var innerTableSql = new StringBuilder();
            var outerTableSql = new StringBuilder();
            this.AddTablesForPagedCollection(selectQuery, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, rootNode);

            // add order by
            var innerOrderSql = new StringBuilder();
            if (selectQuery.OrderClauses.Any()) {
                this.AddOrderByClause(selectQuery.OrderClauses, innerOrderSql, rootNode);
            }
            else {
                this.AppendDefaultOrderBy<T>(rootNode, innerOrderSql);
            }

            // construct the query
            var innerSql = new StringBuilder("select ");
            innerSql.Append(innerColumnSql).Append(innerTableSql).Append(whereSql).Append(innerOrderSql);
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
            sql.Append("select ").Append(outerColumnSql).Append(" from (").Append(innerSql).Append(") as i").Append(outerTableSql);
            var outerOrderSql = new StringBuilder();
            if (selectQuery.OrderClauses.Any()) {
                this.AddOrderByClause(
                    selectQuery.OrderClauses,
                    outerOrderSql,
                    rootNode,
                    (c, n) => "i",
                    (c, n) => n == null ? c.Name + "t" : c.Name + n.Alias);
            }
            else {
                this.AppendDefaultOrderBy<T>(rootNode, outerOrderSql, "i");
            }

            sql.Append(outerOrderSql);
            return rootNode;
        }

        private FetchNode GenerateNoPagingSql<T>(
            SelectQuery<T> selectQuery,
            bool enforceAlias,
            FetchNode rootNode,
            StringBuilder sql,
            int numberCollectionFetches,
            ref DynamicParameters parameters) {
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            var whereSql = new StringBuilder();
            var orderSql = new StringBuilder();
            if (rootNode == null && enforceAlias) {
                rootNode = new FetchNode { Alias = "t" };
            }

            // add where clause
            parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            // add select columns
            this.AddRootColumns(selectQuery, columnSql, rootNode); // do columns second as we may not be fetching but need joins for the where clause

            // add in the tables
            this.AddTables(selectQuery, tableSql, columnSql, rootNode);

            // add order by
            if (selectQuery.OrderClauses.Any()) {
                this.AddOrderByClause(selectQuery.OrderClauses, orderSql, rootNode);
            }
            else if (selectQuery.SkipN > 0) {
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

        private void AppendDefaultOrderBy<T>(FetchNode rootNode, StringBuilder orderSql, string alias = null, string name = null) {
            orderSql.Append(" order by ");
            if (rootNode != null) {
                orderSql.Append(alias ?? rootNode.Alias);
                orderSql.Append('.');
            }

            this.Dialect.AppendQuotedName(orderSql, name ?? this.Configuration.GetMap<T>().PrimaryKey.DbName);
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
                        var signature = this.AddNode(node.Value, tableSql, columnSql);
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

        private void AddTablesForPagedCollection<T>(SelectQuery<T> selectQuery, StringBuilder innerTableSql, StringBuilder outerTableSql, StringBuilder innerColumnSql, StringBuilder outerColumnSql, FetchNode rootNode) {
            innerTableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(innerTableSql, this.Configuration.GetMap<T>());
            innerTableSql.Append(" as t");

            if (selectQuery.IsForUpdate) {
                this.Dialect.AppendForUpdateUsingTableHint(innerTableSql);
            }

            if (rootNode.Children.Any()) {
                // go through the tree and generate the sql
                var signatureBuilder = new StringBuilder();
                var splitOns = new List<string>();
                foreach (var node in rootNode.Children) {
                    var signature = this.AddNodeForPagedCollection(node.Value, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, false);
                    if (node.Value.IsFetched) {
                        signatureBuilder.Append(signature.Signature);
                        splitOns.AddRange(signature.SplitOn);
                    }
                }

                rootNode.FetchSignature = signatureBuilder.ToString();
                rootNode.SplitOn = string.Join(",", splitOns);
            }
        }

        private AddNodeResult AddNodeForPagedCollection(FetchNode node, StringBuilder innerTableSql, StringBuilder outerTableSql, StringBuilder innerColumnSql, StringBuilder outerColumnSql, bool isAlongCollectionBranch) {
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
                outerTableSql.Append(" as ").Append(node.Alias);

                if (node.Column.Relationship == RelationshipType.ManyToOne || node.Column.Relationship == RelationshipType.OneToOne) {
                    outerTableSql.Append(" on ")
                             .Append(node.Parent.Alias)
                             .Append(".")
                             .Append(node.Column.DbName)
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
                innerTableSql.Append(" as ").Append(node.Alias);
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
                foreach (var column in map.OwnedColumns().Where(c => !node.Children.ContainsKey(c.Name))) {
                    if (isNowAlongCollectionBranch) {
                        outerColumnSql.Append(", ");
                        this.AddColumn(outerColumnSql, column, node.Alias);
                    }
                    else {
                        innerColumnSql.Append(", ");
                        this.AddColumn(innerColumnSql, column, node.Alias, column.Name + node.Alias);
                        outerColumnSql.Append(", ").Append("i.").Append(column.Name).Append(node.Alias).Append(" as ");
                        this.Dialect.AppendQuotedName(outerColumnSql, column.Name);
                    }
                }
            }

            // add its children
            var signatureBuilder = new StringBuilder();
            foreach (var child in node.Children) {
                var signature = this.AddNodeForPagedCollection(child.Value, innerTableSql, outerTableSql, innerColumnSql, outerColumnSql, isNowAlongCollectionBranch);
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

        private AddNodeResult AddNode(FetchNode node, StringBuilder tableSql, StringBuilder columnSql) {
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

            tableSql.Append(" left join ");
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

        private void AddRootColumns<T>(SelectQuery<T> selectQuery, StringBuilder columnSql, FetchNode rootNode, bool removeTrailingComma = true) {
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

        private void AddColumn(StringBuilder sql, IColumn column, string tableAlias = null, string columnAlias = null) {
            // add the table alias
            if (tableAlias != null) {
                sql.Append(tableAlias + ".");
            }

            // add the column name
            this.Dialect.AppendQuotedName(sql, column.DbName);

            // add a column alias if required
            if (columnAlias != null) {
                sql.Append(" as ").Append(columnAlias);
            }
            else if (column.DbName != column.Name && column.Relationship == RelationshipType.None) {
                sql.Append(" as " + column.Name);
            }
        }

        private class AddNodeResult {
            public string Signature { get; set; }

            public IList<string> SplitOn { get; set; }
        }
    }
}