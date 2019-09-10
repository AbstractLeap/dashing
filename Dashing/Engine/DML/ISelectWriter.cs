namespace Dashing.Engine.DML {
    using System.Collections.Generic;

    public interface ISelectWriter {
        SelectWriterResult GenerateSql<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> projectedSelectQuery)
            where TBase : class, new();

        SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery, AutoNamingDynamicParameters parameters = null, bool enforceAlias = false) where T : class, new();

        SqlWriterResult GenerateGetSql<T, TPrimaryKey>(TPrimaryKey id);

        SqlWriterResult GenerateGetSql<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);
    }
}