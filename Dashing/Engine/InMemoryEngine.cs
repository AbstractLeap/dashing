namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
#if COREFX
    using System.Reflection;
#endif
    using System.Text;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Engine.InMemory;

    public class InMemoryEngine : IEngine {
        private readonly Dictionary<Type, object> tables;

        private readonly IConfiguration configuration;

        public InMemoryEngine(IConfiguration configuration) {
            this.configuration = configuration;
            this.tables = new Dictionary<Type, object>();
        }

        public IConfiguration Configuration => this.configuration;

        public InMemoryTable<TEntity, TPrimaryKey> GetTable<TEntity, TPrimaryKey>() where TEntity : class, new() {
            if (this.tables.TryGetValue(typeof(TEntity), out var table)) {
                return (InMemoryTable<TEntity, TPrimaryKey>)table;
            }

            throw new Exception($"The type {typeof(TEntity).FullName} is not part of the configuration");
        }

        public T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new() {
            this.AssertConfigured();
            return this.GetTable<T, TPrimaryKey>().Get(id);
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            this.AssertConfigured();
            return ids.Select(id => this.Query<T, TPrimaryKey>(connection, transaction, id));
        }

        public IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var whereClauseNullCheckRewriter = new WhereClauseNullCheckRewriter();
            var whereClauseOpEqualityRewriter = new WhereClauseOpEqualityRewriter();
            var fetchCloner = new FetchCloner(this.Configuration);
            var enumerable =
                typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap(typeof(T)).PrimaryKey.Type)
                                        .GetMethod("Query")
                                        .Invoke(table, new object[0]) as IEnumerable<T>;

            var fetchParser = new FetchTreeParser(this.Configuration);
            var fetchTree = fetchParser.GetFetchTree(query, out _, out _);

            // we may have to query across non-fetched stuff
            var baseWriter = new BaseWriter(new SqlServer2012Dialect(), this.configuration);
            baseWriter.AddWhereClause(query.WhereClauses, new StringBuilder(), new AutoNamingDynamicParameters(), ref fetchTree);

            // note that this fetches all the things in the tree as the whereclause may reference things not fetched
            if (fetchTree != null) {
                enumerable = enumerable.Fetch(fetchTree, this.tables);
            }

            foreach (var whereClause in query.WhereClauses) {
                var rewrittenWhereClause = whereClauseNullCheckRewriter.Rewrite(whereClause);
                rewrittenWhereClause = whereClauseOpEqualityRewriter.Rewrite(rewrittenWhereClause);
                enumerable = enumerable.Where(rewrittenWhereClause.Compile());
            }

            var firstOrderClause = true;
            foreach (var orderClause in query.OrderClauses) {
                var expr = ((LambdaExpression)orderClause.Expression).Compile();
                var funcName = firstOrderClause
                                   ? (orderClause.Direction == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending")
                                   : (orderClause.Direction == ListSortDirection.Ascending ? "ThenBy" : "ThenByDescending");
                var orderBy =
                    typeof(Enumerable).GetMethods()
                                      .Single(
                                          m =>
                                          m.GetParameters().Count() == 2
                                          && m.Name == funcName).MakeGenericMethod(typeof(T), ((LambdaExpression)orderClause.Expression).ReturnType);
                enumerable = (IEnumerable<T>)orderBy.Invoke(null, new object[] {enumerable, expr });
                firstOrderClause = false;
            }

            if (query.SkipN > 0) {
                enumerable = enumerable.Skip(query.SkipN);
            }

            if (query.TakeN > 0) {
                enumerable = enumerable.Take(query.TakeN);
            }

            foreach (var entity in enumerable) {
                yield return fetchCloner.Clone(query, entity);
            }
        }

        public Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            var take = query.TakeN;
            var skip = query.SkipN;
            query.GetType().GetProperty(nameof(SelectQuery<T>.TakeN)).SetMethod.Invoke(query, new object[] { 0 });
            query.GetType().GetProperty(nameof(SelectQuery<T>.SkipN)).SetMethod.Invoke(query, new object[] { 0 });
            var items = this.Query(connection, transaction, query).ToArray();
            return new Page<T> {
                Items = items.Skip(skip).Take(take),
                Skipped = skip,
                Taken = take,
                TotalResults = items.Length
            };
        }

        public IEnumerable<TProjection> Query<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            return this.Query(connection, transaction, query.BaseSelectQuery).Select(query.ProjectionExpression.Compile());
        }

        public Page<TProjection> QueryPaged<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var pagedBase = this.QueryPaged(connection, transaction, query.BaseSelectQuery);
            return new Page<TProjection> {
                                             Items = pagedBase.Items.Select(query.ProjectionExpression.Compile()),
                                             Skipped = pagedBase.Skipped,
                                             Taken = pagedBase.Taken,
                                             TotalResults = pagedBase.TotalResults
                                         };
        }

        public int Count<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            return this.Query(connection, transaction, query).Count();
        }

        public int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return this.ExecuteCUD(entities, nameof(InMemoryTable<T, T>.Insert));
        }

        public int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return this.ExecuteCUD(entities, nameof(InMemoryTable<T, T>.Update));
        }

        public int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return this.ExecuteCUD(entities, nameof(InMemoryTable<T, T>.Delete));
        }

        private int ExecuteCUD<T>(IEnumerable<T> entities, string methodName)
            where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var method = typeof(InMemoryTable<,>).MakeGenericType(
                                                     typeof(T),
                                                     this.Configuration.GetMap<T>().PrimaryKey.Type)
                                                 .GetMethod(methodName);
            var results = 0;
            foreach (var entity in entities) {
                results += (int)method.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            var entitiesToUpdate = this.GetEntitiesUsingPredicates(connection, transaction, predicates);
            foreach (var entity in entitiesToUpdate) {
                update(entity);
            }

            return this.Save(connection, transaction, entitiesToUpdate);
        }

        public int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            var entitiesToDelete = this.GetEntitiesUsingPredicates(connection, transaction, predicates);
            return this.Delete(connection, transaction, entitiesToDelete);
        }

        private T[] GetEntitiesUsingPredicates<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new() {
            this.AssertConfigured();
            var query = new SelectQuery<T>(new NonExecutingSelectQueryExecutor());
            if (predicates != null) {
                foreach (var predicate in predicates) {
                    query.Where(predicate);
                }
            }

            return this.Query(connection, transaction, query).ToArray();
        }

        public Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new() {
            return Task.FromResult(this.Query<T, TPrimaryKey>(connection, transaction, id));
        }

        public Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return Task.FromResult(this.Query<T, TPrimaryKey>(connection, transaction, ids));
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult(this.Query<T>(connection, transaction, query));
        }

        public Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult(this.QueryPaged(connection, transaction, query));
        }

        public async Task<IEnumerable<TProjection>> QueryAsync<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var baseResult = await this.QueryAsync(connection, transaction, query.BaseSelectQuery);
            return baseResult.Select(query.ProjectionExpression.Compile());
        }

        public async Task<Page<TProjection>> QueryPagedAsync<TBase, TProjection>(IDbConnection connection, IDbTransaction transaction, ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new() {
            var pagedBase = await this.QueryPagedAsync(connection, transaction, query.BaseSelectQuery);
            return new Page<TProjection>
                   {
                       Items = pagedBase.Items.Select(query.ProjectionExpression.Compile()),
                       Skipped = pagedBase.Skipped,
                       Taken = pagedBase.Taken,
                       TotalResults = pagedBase.TotalResults
                   };
        }

        public Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult(this.Count(connection, transaction, query));
        }

        public Task<int> InsertAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult(this.Insert(connection, transaction, entities));
        }

        public Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult(this.Save(connection, transaction, entities));
        }

        public Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult(this.Delete(connection, transaction, entities));
        }

        public Task<int> ExecuteAsync<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult(this.Execute(connection, transaction, update, predicates));
        }

        public Task<int> ExecuteBulkDeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult(this.ExecuteBulkDelete(connection, transaction, predicates));
        }

        public IDapper CreateDapperWrapper(ISession session) {
            return new MockDapper();
        }

        private void AssertConfigured() {
            if (this.configuration == null) {
                throw new Exception("Configuration has not been set");
            }

            if (this.tables.Count == 0) {
                // create the in memory tables
                foreach (var map in this.configuration.Maps.Where(m => m.PrimaryKey != null)) {
                    var tableType = typeof(InMemoryTable<,>).MakeGenericType(map.Type, map.PrimaryKey.Type);
                    this.tables.Add(map.Type, Activator.CreateInstance(tableType, new object[] { this.configuration }));
                }
            }
        }
    }
}