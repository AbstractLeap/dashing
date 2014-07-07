namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq.Expressions;

    using Dashing.Configuration;

    public interface IEngine {
        IConfiguration Configuration { get; set; }

        void UseMaps(IDictionary<Type, IMap> maps);

        IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query);

        int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query);

        int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query);

        int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query);

        T Get<T>(IDbConnection connection, int id, bool? asTracked);

        T Get<T>(IDbConnection connection, Guid id, bool? asTracked);

        IEnumerable<T> Get<T>(IDbConnection connection, IEnumerable<int> ids, bool? asTracked);

        IEnumerable<T> Get<T>(IDbConnection connection, IEnumerable<Guid> ids, bool? asTracked);

        void Execute<T>(
            IDbConnection dbConnection, 
            Action<T> update, 
            IEnumerable<Expression<Func<T, bool>>> predicates);

        void ExecuteBulkDelete<T>(
            IDbConnection connection, 
            IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}