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
            int aliasCounter;
            int numberCollectionFetches;
            var rootNode = this.fetchTreeParser.GetFetchTree(selectQuery, out aliasCounter, out numberCollectionFetches);

            // add where clause
            var whereSql = new StringBuilder();
            var parameters = new AutoNamingDynamicParameters();
            this.AddWhereClause(selectQuery.WhereClauses, whereSql, parameters, ref rootNode);

            // add in the tables
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            this.AddTables(selectQuery, tableSql, columnSql, rootNode, false);

            var sql = new StringBuilder(15 + tableSql.Length + whereSql.Length);
            sql.Append("select count(");

            if (numberCollectionFetches == 0) {
                sql.Append("1");
            }
            else {
                sql.Append("distinct ");

                if (rootNode != null) {
                    sql.Append(rootNode.Alias);
                    sql.Append('.');
                }

                this.Dialect.AppendQuotedName(sql, this.Configuration.GetMap<T>().PrimaryKey.DbName);
            }

            sql.Append(")");
            sql.Append(tableSql);
            sql.Append(whereSql);

            return new SqlWriterResult(sql.ToString(), parameters);
        }
    }
}