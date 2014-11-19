namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dapper;

    /// <summary>
    /// Wraps the extension methods offered by the Dapper SqlMapper 
    /// </summary>
    public interface IDapper {
        /// <summary>
        /// Execute parameterized SQL  
        /// </summary>
        /// <returns>Number of rows affected</returns>
        int Execute(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Execute parameterized SQL  
        /// </summary>
        /// <returns>Number of rows affected</returns>
        int Execute(CommandDefinition command);

        /// <summary>
        /// Execute parameterized SQL
        /// </summary>
        /// <returns>Number of rows affected</returns>
        Task<int> ExecuteAsync(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Execute parameterized SQL
        /// </summary>
        /// <returns>Number of rows affected</returns>
        Task<int> ExecuteAsync(CommandDefinition command);

        /// <summary>
        /// Execute parameterized SQL and return an <see cref="IDataReader"/>
        /// </summary>
        /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
        /// or <see cref="DataSet"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// DataTable table = new DataTable("MyTable");
        /// using (var reader = ExecuteReader(cnn, sql, param))
        /// {
        ///     table.Load(reader);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        IDataReader ExecuteReader(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Execute parameterized SQL and return an <see cref="IDataReader"/>
        /// </summary>
        /// <returns>An <see cref="IDataReader"/> that can be used to iterate over the results of the SQL query.</returns>
        /// <remarks>
        /// This is typically used when the results of a query are not processed by Dapper, for example, used to fill a <see cref="DataTable"/>
        /// or <see cref="DataSet"/>.
        /// </remarks>
        IDataReader ExecuteReader(CommandDefinition command);

        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        IEnumerable<dynamic> Query(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        IEnumerable<T> Query<T>(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        IEnumerable<T> Query<T>(CommandDefinition command);

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 4 input parameters
        /// </summary>
        IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 5 input parameters
        /// </summary>
        IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 6 input parameters
        /// </summary>
        IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 7 input parameters
        /// </summary>
        IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        Task<IEnumerable<dynamic>> QueryAsync(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, dynamic param = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes a query, returning the data typed as per T
        /// </summary>
        /// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        /// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        /// </returns>
        Task<IEnumerable<T>> QueryAsync<T>(CommandDefinition command);

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 4 input parameters
        /// </summary>
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 5 input parameters
        /// </summary>
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 6 input parameters
        /// </summary>
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Perform a multi mapping query with 7 input parameters
        /// </summary>
        Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, dynamic param = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn
        /// </summary>
        SqlMapper.GridReader QueryMultiple(string sql, dynamic param = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn
        /// </summary>
        SqlMapper.GridReader QueryMultiple(CommandDefinition command);
    }
}