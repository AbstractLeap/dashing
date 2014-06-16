namespace TopHat.Engine {
    public interface IEntitySqlWriter {
        SqlWriterResult GenerateSql<T>(EntityQueryBase<T> query);
    }
}