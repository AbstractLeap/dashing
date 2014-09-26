namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public interface IEngine {
        ISqlDialect SqlDialect { get; }

        IConfiguration Configuration { get; set; }

        T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id);

        IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> QueryTracked<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query);

        int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities);

        int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}