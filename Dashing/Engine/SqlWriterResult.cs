namespace Dashing.Engine {
    using Dapper;

    public class SqlWriterResult {
        public string Sql { get; private set; }

        public DynamicParameters Parameters { get; private set; }

        public SqlWriterResult(string sql, DynamicParameters parameters) {
            this.Parameters = parameters;
            this.Sql = sql;
        }
    }
}