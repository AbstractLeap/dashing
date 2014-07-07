namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    /// <summary>
    ///     The Session interface.
    /// </summary>
    public interface ISession : IDisposable {
        IDbConnection Connection { get; }

        IDbTransaction Transaction { get; }

        void Complete();

        T Get<T>(int id, bool? asTracked = null);

        T Get<T>(Guid id, bool? asTracked = null);

        IEnumerable<T> Get<T>(IEnumerable<int> ids, bool? asTracked = null);

        IEnumerable<T> Get<T>(IEnumerable<Guid> ids, bool? asTracked = null);

        ISelectQuery<T> Query<T>();

        int Insert<T>(params T[] entities);

        int Insert<T>(IEnumerable<T> entities);

        int Update<T>(params T[] entities);

        int Update<T>(IEnumerable<T> entities);

        int Delete<T>(params T[] entities);

        int Delete<T>(IEnumerable<T> entities);

        void UpdateAll<T>(Action<T> update);

        void Update<T>(Action<T> update, Expression<Func<T, bool>> predicate);

        void Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        void Update<T>(Action<T> update, params Expression<Func<T, bool>>[] predicates);

        void DeleteAll<T>();

        void Delete<T>(Expression<Func<T, bool>> predicate);

        void Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates);

        void Delete<T>(params Expression<Func<T, bool>>[] predicates);
    }
}