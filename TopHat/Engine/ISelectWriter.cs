namespace TopHat.Engine {
    public interface ISelectWriter {
        /// <summary>
        ///     Generate the sql for a select query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        SelectWriterResult GenerateSql<T>(SelectQuery<T> selectQuery);
    }
}