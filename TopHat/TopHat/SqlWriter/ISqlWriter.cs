namespace TopHat.SqlWriter
{
    public interface ISqlWriter
    {
        void Execute<T>(Query<T> query);
    }
}