namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    public interface IEngine {
        IConfiguration Configuration { get; set; }

        IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> QueryTracked<T, TPrimaryKey>(IDbConnection connection, IEnumerable<TPrimaryKey> ids);

        IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query);

        int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query);

        int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query);

        int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query);

        int Execute<T>(IDbConnection dbConnection, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates);

        int ExecuteBulkDelete<T>(IDbConnection connection, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}