namespace TopHat.Engine {
    using Dapper;

    public class SqlWriterResult {
        public string Sql { get; private set; }

        public DynamicParameters Parameters { get; private set; }

        public FetchNode FetchTree { get; private set; }

        public SqlWriterResult(string sql, DynamicParameters parameters, FetchNode fetchTree) {
            this.Sql = sql;
            this.Parameters = parameters;
            this.FetchTree = fetchTree;
        }
    }
}
