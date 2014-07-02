namespace TopHat.Engine {
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using TopHat.Configuration;

    internal class SelectWriter : BaseWriter, ISelectWriter {
        public SelectWriter(ISqlDialect dialect, IConfiguration config)
            : this(dialect, new WhereClauseWriter(dialect, config), config) { }

        public SelectWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) { }

        private static ConcurrentDictionary<Tuple<Type, string>, string> queryCache = new ConcurrentDictionary<Tuple<Type, string>, string>();

        public SqlWriterResult GenerateGetSql<T>(int id) {
            var sql = queryCache.GetOrAdd(Tuple.Create<Type, string>(typeof(T), "GetSingle"), k => this.GenerateGetSql<T>(false));
            return new SqlWriterResult(sql, new DynamicParameters(new { Id = id }));
        }

        public SqlWriterResult GenerateGetSql<T>(System.Guid id) {
            var sql = queryCache.GetOrAdd(Tuple.Create<Type, string>(typeof(T), "GetSingle"), k => this.GenerateGetSql<T>(false));
            return new SqlWriterResult(sql, new DynamicParameters(new { Id = id }));
        }

        public SqlWriterResult GenerateGetSql<T>(IEnumerable<int> ids) {
            var sql = queryCache.GetOrAdd(Tuple.Create<Type, string>(typeof(T), "GetMultiple"), k => this.GenerateGetSql<T>(true));
            return new SqlWriterResult(sql, new DynamicParameters(new { Ids = ids }));
        }

        public SqlWriterResult GenerateGetSql<T>(IEnumerable<System.Guid> ids) {
            var sql = queryCache.GetOrAdd(Tuple.Create<Type, string>(typeof(T), "GetMultiple"), k => this.GenerateGetSql<T>(true));
            return new SqlWriterResult(sql, new DynamicParameters(new { Ids = ids }));
        }

        private string GenerateGetSql<T>(bool isMultiple) {
            var sql = new StringBuilder("select ");
            var map = this.Configuration.GetMap<T>();
            foreach (var column in map.Columns) {
                if (!column.Value.IsIgnored && !column.Value.IsExcludedByDefault
                        && (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne)) {
                    this.AddColumn(sql, column.Value, string.Empty);
                    sql.Append(", ");
                }
            }

            sql.Remove(sql.Length - 2, 2);
            sql.Append(" from ");
            this.Dialect.AppendQuotedTableName(sql, map);
            if (isMultiple) {
                sql.Append(" where " + map.PrimaryKey.Name + " in @Ids");
            }
            else {
                sql.Append(" where " + map.PrimaryKey.Name + " = @Id");
            }

            return sql.ToString();
        }

        public SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery) {
            var sql = new StringBuilder();
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            var whereSql = new StringBuilder();
            var orderSql = new StringBuilder();

            // get fetch tree structure
            int aliasCounter;
            var rootNode = this.GetFetchTree(selectQuery, out aliasCounter);

            // add select columns
            this.AddColumns(selectQuery, columnSql, rootNode);

            // add where clause
            var parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            // add in the tables
            this.AddTables(selectQuery, tableSql, columnSql, rootNode);

            // add order by
            this.AddOrderByClause(selectQuery.OrderClauses, orderSql);

            // construct the query
            sql.Append("select ");
            sql.Append(columnSql);
            sql.Append(tableSql);
            sql.Append(whereSql);
            sql.Append(orderSql);

            return new SelectWriterResult(sql.ToString(), parameters, rootNode);
        }

        private FetchNode GetFetchTree<T>(SelectQuery<T> selectQuery, out int aliasCounter) {
            FetchNode rootNode = null;
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
                        int numNames = entityNames.Count;
                        while (numNames > 0) {
                            var propName = entityNames.Pop();

                            // don't add duplicates
                            if (!currentNode.Children.ContainsKey(propName)) {
                                // add to tree
                                var node = new FetchNode {
                                    Parent = currentNode,
                                    Column = this.Configuration.GetMap(currentNode == rootNode ? typeof(T) : currentNode.Column.Type).Columns[propName],
                                    Alias = "t_" + ++aliasCounter,
                                    IsFetched = true
                                };
                                currentNode.Children.Add(propName, node);
                                currentNode = node;
                            }
                            else {
                                currentNode = currentNode.Children[propName];
                            }

                            numNames--;
                        }
                    }
                }
            }

            return rootNode;
        }

        private void AddTables<T>(SelectQuery<T> selectQuery, StringBuilder tableSql, StringBuilder columnSql, FetchNode rootNode) {
            // separate string builder for the tables as we use the sql builder for fetch columns
            tableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Configuration.GetMap<T>());

            if (rootNode != null && rootNode.Children.Any()) {
                tableSql.Append(" as t");

                // now let's go through the tree and generate the sql
                var signatureBuilder = new StringBuilder();
                var splitOns = new List<string>();
                foreach (var node in rootNode.Children.OrderBy(c => c.Value.Column.FetchId)) {
                    var signature = this.AddNode(node.Value, tableSql, columnSql);
                    if (node.Value.IsFetched) {
                        signatureBuilder.Append(node.Value.Column.FetchId + "S" + signature.Signature + "E");
                        splitOns.AddRange(signature.SplitOn);
                    }
                }

                rootNode.FetchSignature = signatureBuilder.ToString();
                rootNode.SplitOn = string.Join(",", splitOns);
            }
        }

        private AddNodeResult AddNode(FetchNode node, StringBuilder tableSql, StringBuilder columnSql) {
            // add this node and then it's children
            // add table sql
            var splitOns = new List<string>();
            if (node.IsFetched) {
                splitOns.Add(this.Configuration.GetMap(node.Column.Type).PrimaryKey.Name);
            }

            tableSql.Append(" left join ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Configuration.GetMap(node.Column.Type));
            tableSql.Append(" as " + node.Alias);
            tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.DbName + " = " + node.Alias + "." + this.Configuration.GetMap(node.Column.Type).PrimaryKey.DbName);

            // add the columns
            if (node.IsFetched) {
                foreach (var column in this.Configuration.GetMap(node.Column.Type).Columns) {
                    // only include the column if not excluded and not fetched subsequently
                    if (!column.Value.IsIgnored && !column.Value.IsExcludedByDefault && !node.Children.ContainsKey(column.Key)
                        && (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne)) {
                        columnSql.Append(", ");
                        this.AddColumn(columnSql, column.Value, node.Alias);
                    }
                }
            }

            // add its children
            var signatureBuilder = new StringBuilder();
            foreach (var child in node.Children.OrderBy(c => c.Value.Column.FetchId)) {
                var signature = this.AddNode(child.Value, tableSql, columnSql);
                if (child.Value.IsFetched) {
                    signatureBuilder.Append(child.Value.Column.FetchId + "S" + signature.Signature + "E");
                    splitOns.AddRange(signature.SplitOn);
                }
            }

            return new AddNodeResult { Signature = signatureBuilder.ToString(), SplitOn = splitOns };
        }

        private void AddColumns<T>(SelectQuery<T> selectQuery, StringBuilder columnSql, FetchNode rootNode) {
            string alias = string.Empty;
            if (selectQuery.Fetches.Any()) {
                alias = "t";
            }

            if (selectQuery.Projection == null) {
                foreach (var column in this.Configuration.GetMap<T>().Columns) {
                    if (!column.Value.IsIgnored && (selectQuery.FetchAllProperties || !column.Value.IsExcludedByDefault)
                        && (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne)
                        && (rootNode == null || !rootNode.Children.ContainsKey(column.Key))) {
                        this.AddColumn(columnSql, column.Value, alias);
                        columnSql.Append(", ");
                    }
                }
            }

            columnSql.Remove(columnSql.Length - 2, 2);
        }

        private void AddColumn(StringBuilder sql, IColumn column, string alias = "") {
            if (alias.Length > 0) {
                sql.Append(alias + ".");
            }

            this.Dialect.AppendQuotedName(sql, column.DbName);
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