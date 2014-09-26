namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    using Dashing.Engine.DML;

    public interface IGeneratedCodeManager {
        /// <summary>
        ///     Returns a type for a Foreign Key class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Type GetForeignKeyType(Type type);

        /// <summary>
        ///     Returns a type for a Tracked class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Type GetTrackingType(Type type);

        /// <summary>
        ///     Returns a type for the Update class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Type GetUpdateType(Type type);

        /// <summary>
        ///     Returns an instance of a Foreign Key class for the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateForeignKeyInstance<T>();

        /// <summary>
        ///     Returns an instance of a tracking class for the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateTrackingInstance<T>();

        /// <summary>
        ///     Returns an instance of an update class for the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateUpdateInstance<T>();

        /// <summary>
        ///     Convenience method for enabling tracking on a tracking instance
        /// </summary>
        void TrackInstance<T>(T entity);

        /// <summary>
        ///     Calls the correct static function within the generated code for a particular fetch tree
        /// </summary>
        IEnumerable<T> Query<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        ///     Calls the correct static function within the generated code for a particular fetch tree
        /// </summary>
        IEnumerable<T> Query<T>(SqlWriterResult result, IDbConnection connection, IDbTransaction transaction, bool asTracked = false);

        /// <summary>
        ///     Calls the correct static function within the generated code for a particular fetch tree
        /// </summary>
        IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, string sql, dynamic parameters = null);

        /// <summary>
        ///     Executes some sql against the connection (wraps Dapper method)
        /// </summary>
        int Execute(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null);

        /// <summary>
        ///     Executes some sql against the connection (wraps Dapper method)
        /// </summary>
        T QueryScalar<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null);
        /// <summary>
        ///     Calls the correct static function within the generated code for a particular fetch tree
        /// </summary>
        Task<IEnumerable<T>> QueryAsync<T>(SelectWriterResult result, SelectQuery<T> query, IDbConnection connection, IDbTransaction transaction);

        Task<IEnumerable<T>> QueryAsync<T>(SqlWriterResult sqlQuery, IDbConnection connection, IDbTransaction transaction, bool asTracked = false);

        /// <summary>
        ///     Calls the correct static function within the generated code for a particular fetch tree
        /// </summary>
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, string sql, dynamic parameters = null);

        /// <summary>
        ///     Executes some sql against the connection (wraps Dapper method)
        /// </summary>
        Task<int> ExecuteAsync(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null);

        /// <summary>
        ///     Executes some sql against the connection (wraps Dapper method)
        /// </summary>
        Task<T> QueryScalarAsync<T>(string sql, IDbConnection connection, IDbTransaction transaction, dynamic param = null);
    }
}