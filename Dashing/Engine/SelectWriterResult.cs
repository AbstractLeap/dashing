namespace Dashing.Engine {
    using Dapper;

    public class SelectWriterResult : SqlWriterResult {
        public FetchNode FetchTree { get; private set; }

        public SelectWriterResult(string sql, DynamicParameters parameters, FetchNode fetchTree)
            : base(sql, parameters) {
            this.FetchTree = fetchTree;
        }
    }
}