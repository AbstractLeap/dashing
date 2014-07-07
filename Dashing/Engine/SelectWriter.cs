namespace Dashing.Engine {
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
            : this(dialect, new WhereClauseWriter(dialect, config), config) {
        }

        public SelectWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) {
        }

        private static readonly ConcurrentDictionary<Tuple<Type, string>, string> QueryCache = new ConcurrentDictionary<Tuple<Type, string>, string>();

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
            // TODO: one StringBuilder to rule them all
            var sql = new StringBuilder();
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            var whereSql = new StringBuilder();
            var orderSql = new StringBuilder();

            // get fetch tree structure
            int aliasCounter;
            bool hasCollectionFetches;
            var rootNode = this.GetFetchTree(selectQuery, out aliasCounter, out hasCollectionFetches);

            // add select columns
            this.AddColumns(selectQuery, columnSql, rootNode);

            // add where clause
            var parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

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
            if (selectQuery.TakeN > 0 || selectQuery.SkipN > 0) {
                if (parameters == null) {
                    parameters = new DynamicParameters();
                }

                this.Dialect.ApplyPaging(sql, orderSql, selectQuery.TakeN, selectQuery.SkipN);
                if (selectQuery.TakeN > 0) {
                    parameters.Add("@take", selectQuery.TakeN);
                }

                if (selectQuery.SkipN > 0) {
                    parameters.Add("@skip", selectQuery.SkipN);
                }
            }

            return new SelectWriterResult(sql.ToString(), parameters, rootNode) { HasCollectionFetches = hasCollectionFetches };
        }

        private FetchNode GetFetchTree<T>(SelectQuery<T> selectQuery, out int aliasCounter, out bool hasCollectionFetches) {
            FetchNode rootNode = null;
            hasCollectionFetches = false;
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
                                var column = this.Configuration.GetMap(currentNode == rootNode ? typeof(T) : currentNode.Column.Type).Columns[propName];
                                if (column.Relationship == RelationshipType.OneToMany) {
                                    hasCollectionFetches = true;
                                }

                                // add to tree
                                var node = new FetchNode { Parent = currentNode, Column = column, Alias = "t_" + ++aliasCounter, IsFetched = true };
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
            var alias = selectQuery.Fetches.Any() ? "t" : null;

            if (selectQuery.Projection == null) {
                foreach (var column in this.Configuration.GetMap<T>().OwnedColumns(selectQuery.FetchAllProperties).Where(c => rootNode == null || !rootNode.Children.ContainsKey(c.Name))) {
                    this.AddColumn(columnSql, column, alias);
                    columnSql.Append(", ");
                }
            }

            columnSql.Remove(columnSql.Length - 2, 2);
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