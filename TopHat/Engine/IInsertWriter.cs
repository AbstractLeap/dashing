namespace TopHat.Engine {
    public interface IInsertWriter {
        SqlWriterResult GenerateSql<T>(T entity);

        string GenerateGetIdSql<T>();
    }
}