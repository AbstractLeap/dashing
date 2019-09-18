namespace Dashing.Engine.DML {
    public interface ICountWriter {
        SqlWriterResult GenerateCountSql<T>(SelectQuery<T> selectQuery) where T : class, new();

        SqlWriterResult GenerateCountSql<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> projectedSelectQuery)
            where TBase : class, new();
    }
}