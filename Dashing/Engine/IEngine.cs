namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;

    public interface IEngine {
        IConfiguration Configuration { get; set; }

        T Query<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id);

        IEnumerable<T> Query<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> Query<T>(ISessionState sessionState, SelectQuery<T> query);

        Page<T> QueryPaged<T>(ISessionState sessionState, SelectQuery<T> query);

        int Count<T>(ISessionState sessionState, SelectQuery<T> query);

        int Insert<T>(ISessionState sessionState, IEnumerable<T> entities);

        int Save<T>(ISessionState sessionState, IEnumerable<T> entities);

        int Delete<T>(ISessionState sessionState, IEnumerable<T> entities);

        int Execute<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new();

        int ExecuteBulkDelete<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates);

        Task<T> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id);

        Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids);

        Task<IEnumerable<T>> QueryAsync<T>(ISessionState sessionState, SelectQuery<T> query);

        Task<Page<T>> QueryPagedAsync<T>(ISessionState sessionState, SelectQuery<T> query);

        Task<int> CountAsync<T>(ISessionState sessionState, SelectQuery<T> query);

        Task<int> InsertAsync<T>(ISessionState sessionState, IEnumerable<T> entities);

        Task<int> SaveAsync<T>(ISessionState sessionState, IEnumerable<T> entities);

        Task<int> DeleteAsync<T>(ISessionState sessionState, IEnumerable<T> entities);

        Task<int> ExecuteAsync<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();

        Task<int> ExecuteBulkDeleteAsync<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}