namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    public interface IEngine {
        IConfiguration Configuration { get; set; }

        IEnumerable<T> Query<T, TPrimaryKey>(IDbTransaction transaction, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> QueryTracked<T, TPrimaryKey>(IDbTransaction transaction, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> Query<T>(IDbTransaction transaction, SelectQuery<T> query);

        int Execute<T>(IDbTransaction transaction, InsertEntityQuery<T> query);

        int Save<T>(IDbTransaction transaction, IEnumerable<T> entities);

        int Delete<T>(IDbTransaction transaction, IEnumerable<T> entities);

        int Execute<T>(IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        int ExecuteBulkDelete<T>(IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}