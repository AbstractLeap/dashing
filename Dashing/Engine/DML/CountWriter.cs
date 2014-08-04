namespace Dashing.Engine.DML {
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    internal class CountWriter : SelectWriter, ICountWriter {
        public CountWriter(ISqlDialect dialect, IConfiguration configuration)
            : base(dialect, configuration) { }

        public SqlWriterResult GenerateCountSql<T>(SelectQuery<T> selectQuery) {
            // get fetch tree structure
            int aliasCounter;
            int numberCollectionFetches;
            var rootNode = this.GetFetchTree(selectQuery, out aliasCounter, out numberCollectionFetches);

            // add where clause
            var whereSql = new StringBuilder();
            var parameters = this.AddWhereClause(selectQuery.WhereClauses, whereSql, ref rootNode);

            // add in the tables
            var columnSql = new StringBuilder();
            var tableSql = new StringBuilder();
            this.AddTables(selectQuery, tableSql, columnSql, rootNode);

            var sql = new StringBuilder(15 + tableSql.Length + whereSql.Length);
            sql.Append("select count(1)");
            sql.Append(tableSql);
            sql.Append(whereSql);

            return new SqlWriterResult(sql.ToString(), parameters);
        }
    }
}