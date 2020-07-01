namespace Dashing.Engine.DML {
    using Dapper;

    public class SelectWriterResult : SqlWriterResult {
        public QueryTree MapQueryTree { get; internal set; }

        public int NumberCollectionsFetched { get; set; }

        public SelectWriterResult(string sql, DynamicParameters parameters, QueryTree mapQueryTree)
            : base(sql, parameters) {
            this.MapQueryTree = mapQueryTree;
        }
    }
}