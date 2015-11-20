namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;

    public interface IEngine {
        IConfiguration Configuration { get; set; }

        T Query<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id) where T : class, new();

        IEnumerable<T> Query<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids) where T : class, new();

        IEnumerable<T> Query<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new();

        Page<T> QueryPaged<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new();

        int Count<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new();

        int Insert<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new();

        int Save<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new();

        int Delete<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new();

        int Execute<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        int ExecuteBulkDelete<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        Task<T> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids) where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new();

        Task<Page<T>> QueryPagedAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new();

        Task<int> CountAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new();

        Task<int> InsertAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new();

        Task<int> SaveAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new();

        Task<int> DeleteAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new();

        Task<int> ExecuteAsync<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        Task<int> ExecuteBulkDeleteAsync<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();
    }
}