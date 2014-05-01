namespace TopHat.SqlWriter
{
    public struct SqlWriterResult
    {
        public string Sql { get; set; }

        public dynamic Parameters { get; set; }
    }
}