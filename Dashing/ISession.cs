namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;

    public interface ISession : IDisposable {
        IConfiguration Configuration { get; }

        IDapper Dapper { get; }

        void Complete();

        T Get<T, TPrimaryKey>(TPrimaryKey id);

        T GetTracked<T, TPrimaryKey>(TPrimaryKey id);

        IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> GetTracked<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);

        ISelectQuery<T> Query<T>();

        int Insert<T>(IEnumerable<T> entities);

        int Save<T>(IEnumerable<T> entities);

        int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        int Delete<T>(IEnumerable<T> entities);

        int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates);

        int UpdateAll<T>(Action<T> update);

        int DeleteAll<T>();

        Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id);

        Task<T> GetTrackedAsync<T, TPrimaryKey>(TPrimaryKey id);

        Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);

        Task<IEnumerable<T>> GetTrackedAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids);

        Task<int> InsertAsync<T>(IEnumerable<T> entities);

        Task<int> SaveAsync<T>(IEnumerable<T> entities);

        Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        Task<int> DeleteAsync<T>(IEnumerable<T> entities);

        Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates);

        Task<int> UpdateAllAsync<T>(Action<T> update);

        Task<int> DeleteAllAsync<T>();
    }
}