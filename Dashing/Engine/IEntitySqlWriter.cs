namespace Dashing.Engine {
    public interface IEntitySqlWriter {
        SqlWriterResult GenerateSql<T>(EntityQueryBase<T> query);
    }
}