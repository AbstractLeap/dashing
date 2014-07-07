namespace Dashing.Engine.DML {
    public interface IEntitySqlWriter {
        SqlWriterResult GenerateSql<T>(EntityQueryBase<T> query);
    }
}