namespace Dashing.Engine.DML {
    using System.Collections.Generic;

    public interface ISelectWriter {
        SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery, AutoNamingDynamicParameters parameters = null, bool enforceAlias = false) where T : class, new();

        SqlWriterResult GenerateGetSql<T, TPrimaryKey>(TPrimaryKey id);

        SqlWriterResult GenerateGetSql<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);
    }
}