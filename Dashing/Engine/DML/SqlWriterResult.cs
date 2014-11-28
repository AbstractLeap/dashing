namespace Dashing.Engine.DML {
    using Dapper;

    public class SqlWriterResult {
        public string Sql { get; internal set; }

        public DynamicParameters Parameters { get; internal set; }

        public SqlWriterResult(string sql, DynamicParameters parameters) {
            this.Parameters = parameters;
            this.Sql = sql;
        }
    }
}