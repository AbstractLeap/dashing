namespace TopHat.Engine {
    internal interface ISelectWriter {
        /// <summary>
        /// Generate the sql for a select query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectQuery"></param>
        /// <returns></returns>
        string GenerateSql<T>(SelectQuery<T> selectQuery);
    }
}