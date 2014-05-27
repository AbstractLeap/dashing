namespace TopHat.Engine {
    using Dapper;

    internal class WhereClauseWriterResult {
        public string Sql { get; private set; }

        public DynamicParameters Parameters { get; private set; }

        public WhereClauseWriterResult(string sql, DynamicParameters parameters) {
            this.Sql = sql;
            this.Parameters = parameters;
        }
    }
}
