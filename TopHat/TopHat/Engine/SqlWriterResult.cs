namespace TopHat.Engine {
    using Dapper;

    internal class SqlWriterResult {
        public string Sql { get; private set; }

        public DynamicParameters Parameters { get; private set; }

        public SqlWriterResult(string sql, DynamicParameters parameters) {
            this.Sql = sql;
            this.Parameters = parameters;
        }
    }
}
