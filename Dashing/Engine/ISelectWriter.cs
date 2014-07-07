namespace Dashing.Engine {
    using System.Collections.Generic;

    public interface ISelectWriter {
        SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery);

        SqlWriterResult GenerateGetSql<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);
    }
}