namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public interface IEngine {
        ISqlDialect SqlDialect { get; }

        IConfiguration Configuration { get; set; }

        T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id);

        IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        int Count<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates);

        Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id);

        Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids);

        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        Task<int> InsertAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        Task<int> ExecuteAsync<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        Task<int> ExecuteBulkDeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}