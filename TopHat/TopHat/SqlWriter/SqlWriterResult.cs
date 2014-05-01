using Dapper;

namespace TopHat.SqlWriter
{
    public struct SqlWriterResult
    {
        public string Sql { get; set; }

        public DynamicParameters Parameters { get; set; }
    }
}