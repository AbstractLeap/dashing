namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    using TopHat.Configuration;

    internal class SelectWriter : BaseWriter, ISelectWriter {
        public SelectWriter(ISqlDialect dialect, IDictionary<Type, IMap> maps)
            : this(dialect, new WhereClauseWriter(dialect, maps), maps) {}

        public SelectWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IDictionary<Type, IMap> maps)
            : base(dialect, whereClauseWriter, maps) {}

        public SqlWriterResult GenerateSql<T>(SelectQuery<T> selectQuery) {
            var sql = new StringBuilder();
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();

            // add table references
            var rootNode = this.AddTables(selectQuery, tableSql, columnSql);

            // add select columns
            this.AddColumns(selectQuery, sql, tableSql, columnSql, rootNode);

            // add where clause
            var parameters = this.AddWhereClause(selectQuery.WhereClauses, sql, rootNode);

            // add order by
            this.AddOrderByClause(selectQuery.OrderClauses, sql);

            return new SqlWriterResult(sql.ToString(), parameters, rootNode);
        }

        private FetchNode AddTables<T>(SelectQuery<T> selectQuery, StringBuilder tableSql, StringBuilder columnSql) {
            // separate string builder for the tables as we use the sql builder for fetch columns
            tableSql.Append(" from ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Maps[typeof(T)]);

            if (selectQuery.HasFetches()) {
                tableSql.Append(" as t");

                // now we go through the fetches and generate the tree structure
                var rootNode = new FetchNode { Alias = "t" };
                int aliasCounter = 0;
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
                                                             Column = this.Maps[currentNode == rootNode ? typeof(T) : currentNode.Column.Type].Columns[propName],
                                                             Alias = "t_" + ++aliasCounter
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

                // now let's go through the tree and generate the sql
                StringBuilder signatureBuilder = new StringBuilder();
                var splitOns = new List<string>();
                foreach (var node in rootNode.Children.OrderBy(c => c.Value.Column.FetchId)) {
                    var signature = this.AddNode(node.Value, tableSql, columnSql);
                    signatureBuilder.Append(node.Value.Column.FetchId + "S" + signature.Signature + "E");
                    splitOns.AddRange(signature.SplitOn);
                }

                rootNode.FetchSignature = signatureBuilder.ToString();
                rootNode.SplitOn = string.Join(",", splitOns);

                return rootNode;
            }

            return null;
        }

        private AddNodeResult AddNode(FetchNode node, StringBuilder tableSql, StringBuilder columnSql) {
            // add this node and then it's children
            // add table sql
            var splitOns = new List<string>();
            splitOns.Add(this.Maps[node.Column.Type].PrimaryKey.Name);
            tableSql.Append(" left join ");
            this.Dialect.AppendQuotedTableName(tableSql, this.Maps[node.Column.Type]);
            tableSql.Append(" as " + node.Alias);
            tableSql.Append(" on " + node.Parent.Alias + "." + node.Column.DbName + " = " + node.Alias + "." + this.Maps[node.Column.Type].PrimaryKey.DbName);

            // add the columns
            foreach (var column in this.Maps[node.Column.Type].Columns) {
                // only include the column if not excluded and not fetched subsequently
                if (!column.Value.IsIgnored && !column.Value.IsExcludedByDefault && !node.Children.ContainsKey(column.Key)
                    && (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne)) {
                    columnSql.Append(", ");
                    this.AddColumn(columnSql, column.Value, node.Alias);
                }
            }

            // add its children
            StringBuilder signatureBuilder = new StringBuilder();
            foreach (var child in node.Children.OrderBy(c => c.Value.Column.FetchId)) {
                var signature = this.AddNode(child.Value, tableSql, columnSql);
                signatureBuilder.Append(child.Value.Column.FetchId + "S" + signature + "E");
                splitOns.AddRange(signature.SplitOn);
            }

            return new AddNodeResult { Signature = signatureBuilder.ToString(), SplitOn = splitOns};
        }

        private void AddColumns<T>(SelectQuery<T> selectQuery, StringBuilder sql, StringBuilder tableSql, StringBuilder columnSql, FetchNode rootNode) {
            sql.Append("select ");

            string alias = string.Empty;
            if (selectQuery.Fetches.Any()) {
                alias = "t";
            }

            if (selectQuery.Projection == null) {
                foreach (var column in this.Maps[typeof(T)].Columns) {
                    if (!column.Value.IsIgnored && (selectQuery.FetchAllProperties || !column.Value.IsExcludedByDefault)
                        && (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne)
                        && (rootNode == null || !rootNode.Children.ContainsKey(column.Key))) {
                        this.AddColumn(sql, column.Value, alias);
                        sql.Append(", ");
                    }
                }
            }

            // remove the last ,
            sql.Remove(sql.Length - 2, 2);

            sql.Append(columnSql);
            sql.Append(tableSql);
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