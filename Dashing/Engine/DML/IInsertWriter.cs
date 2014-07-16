namespace Dashing.Engine.DML {
    public interface IInsertWriter {
        SqlWriterResult GenerateSql<T>(T entity);

        string GenerateGetIdSql<T>();
    }
}