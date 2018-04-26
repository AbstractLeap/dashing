namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.SqlBuilder;

    public interface IEngine {
        IConfiguration Configuration { get; }

        T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new();

        IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) where T : class, new();

        IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new();

        Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new();

        IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression) where T : class, new();

        int Count<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new();

        int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new();

        int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new();

        int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new();

        int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new();

        Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression) where T : class, new();

        Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new();

        Task<int> InsertAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new();

        Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new();

        Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new();

        Task<int> ExecuteAsync<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        Task<int> ExecuteBulkDeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();
    }
}