namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;

    public interface ISelectWriter {
        /// <summary>
        ///     Generate the sql for a select query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery);

        SqlWriterResult GenerateGetSql<T>(int id);

        SqlWriterResult GenerateGetSql<T>(Guid id);

        SqlWriterResult GenerateGetSql<T>(IEnumerable<int> ids);

        SqlWriterResult GenerateGetSql<T>(IEnumerable<Guid> ids);
    }
}