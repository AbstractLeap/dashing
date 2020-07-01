namespace Dashing.Engine.DML {
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class CountWriter : SelectWriter, ICountWriter {
        public CountWriter(ISqlDialect dialect, IConfiguration configuration)
            : base(dialect, configuration) {
        }

        public SqlWriterResult GenerateCountSql<T>(SelectQuery<T> selectQuery) where T : class, new() {
            // get fetch tree structure
            var rootNode = this.fetchTreeParser.GetFetchTree(selectQuery, out _, out var numberCollectionFetches)
                ?? new QueryTree(false, false, this.Configuration.GetMap<T>());

            return this.InnerGenerateCountSql(selectQuery, rootNode, numberCollectionFetches);
        }

        public SqlWriterResult GenerateCountSql<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> projectedSelectQuery)
            where TBase : class, new() {
            // get fetch tree structure
            var rootNode = this.fetchTreeParser.GetFetchTree(projectedSelectQuery.BaseSelectQuery, out _, out var numberCollectionFetches) 
                           ?? new QueryTree(true, false, this.Configuration.GetMap<TBase>());

            // add in the projection structure
            var selectProjectionParser = new SelectProjectionParser<TBase>(this.Configuration);
            selectProjectionParser.ParseExpression(projectedSelectQuery.ProjectionExpression, rootNode);

            return this.InnerGenerateCountSql(projectedSelectQuery.BaseSelectQuery, rootNode, numberCollectionFetches);
        }

        private SqlWriterResult InnerGenerateCountSql<T>(SelectQuery<T> selectQuery, QueryTree rootQueryNode, int numberCollectionFetches)
            where T : class, new() { // add where clause
            var whereSql = new StringBuilder();
            var parameters = new AutoNamingDynamicParameters();
            var aliasProvider = new DefaultAliasProvider();
            this.AddWhereClause(selectQuery.WhereClauses, whereSql, parameters, aliasProvider, ref rootQueryNode);

            // add in the tables
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            this.AddTables(selectQuery, tableSql, columnSql, rootQueryNode, aliasProvider, false);

            var sql = new StringBuilder(15 + tableSql.Length + whereSql.Length);
            sql.Append("select count(");

            if (numberCollectionFetches == 0) {
                sql.Append("1");
            }
            else {
                sql.Append("distinct ");

                if (rootQueryNode != null) {
                    sql.Append(aliasProvider.GetAlias(rootQueryNode));
                    sql.Append('.');
                }

                this.Dialect.AppendQuotedName(
                    sql,
                    this.Configuration.GetMap<T>()
                        .PrimaryKey.DbName);
            }

            sql.Append(")");
            sql.Append(tableSql);
            sql.Append(whereSql);

            return new SqlWriterResult(sql.ToString(), parameters);
        }
    }
}