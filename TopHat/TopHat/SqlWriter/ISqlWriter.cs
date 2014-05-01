namespace TopHat.SqlWriter
{
    public interface ISqlWriter
    {
        SqlWriterResult Execute<T>(Query<T> query);
    }
}