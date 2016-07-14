namespace Dashing.Testing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;

    public class InMemoryEngine : IEngine {
        private Dictionary<Type, object> tables;

        private IConfiguration configuration;

        public InMemoryEngine() {
            this.tables = new Dictionary<Type, object>();
        }

        public IConfiguration Configuration {
            get {
                return this.configuration;
            }
            set {
                if (this.configuration != null) {
                    throw new Exception("Can only set the configuration once");
                }

                this.configuration = value;
            }
        }

        public InMemoryTable<TEntity, TPrimaryKey> GetTable<TEntity, TPrimaryKey>() where TEntity : class, new() {
            if (!tables.ContainsKey(typeof(TEntity))) {
                throw new Exception(string.Format("The type {0} is not part of the configuration", typeof(TEntity).FullName));
            }

            return (InMemoryTable<TEntity, TPrimaryKey>)this.tables[typeof(TEntity)];
        }

        public T Query<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id) where T : class, new() {
            this.AssertConfigured();
            return this.GetTable<T, TPrimaryKey>().Get(id);
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            this.AssertConfigured();
            return ids.Select(i => this.GetTable<T, TPrimaryKey>().Get(i));
        }

        public IEnumerable<T> Query<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
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
            int numberCollectionFetches;
            int aliasCounter;
            var fetchTree = fetchParser.GetFetchTree(query, out aliasCounter, out numberCollectionFetches);

            // we may have to query across non-fetched stuff
            var baseWriter = new BaseWriter(new SqlServer2012Dialect(), this.configuration);
            baseWriter.AddWhereClause(query.WhereClauses, new StringBuilder(), ref fetchTree);

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

        public Page<T> QueryPaged<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            return new Page<T> {
                Items = this.Query<T>(sessionState, query),
                Skipped = query.SkipN,
                Taken = query.TakeN,
                TotalResults = this.Count(sessionState, query)
            };
        }

        public int Count<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            return this.Query<T>(sessionState, query).Count();
        }

        public int Insert<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var insertMethod = typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap<T>().PrimaryKey.Type).GetMethod("Insert");
            var results = 0;
            foreach (var entity in entities) {
                results += (int)insertMethod.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Save<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var updateMethod = typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap<T>().PrimaryKey.Type).GetMethod("Update");
            var results = 0;
            foreach (var entity in entities) {
                results += (int)updateMethod.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Delete<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var deleteMethod = typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap<T>().PrimaryKey.Type).GetMethod("Delete");
            var results = 0;
            foreach (var entity in entities) {
                results += (int)deleteMethod.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Execute<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.AssertConfigured();
            var query = new SelectQuery<T>(new NonExecutingSelectQueryExecutor());
            if (predicates != null) {
                foreach (var predicate in predicates) {
                    query.Where(predicate);
                }
            }

            var entitiesToUpdate = this.Query<T>(sessionState, query).ToList();
            foreach (var entity in entitiesToUpdate) {
                update(entity);
            }

            return this.Save(sessionState, entitiesToUpdate);
        }

        public int ExecuteBulkDelete<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.AssertConfigured();
            var query = new SelectQuery<T>(new NonExecutingSelectQueryExecutor());
            if (predicates != null) {
                foreach (var predicate in predicates) {
                    query.Where(predicate);
                }
            }

            var entitiesToDelete = this.Query<T>(sessionState, query).ToList();
            return this.Delete(sessionState, entitiesToDelete);
        }

        public Task<T> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, TPrimaryKey id) where T : class, new() {
            return Task.FromResult(this.Query<T, TPrimaryKey>(sessionState, id));
        }

        public Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(ISessionState sessionState, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return Task.FromResult(this.Query<T, TPrimaryKey>(sessionState, ids));
        }

        public Task<IEnumerable<T>> QueryAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult(this.Query<T>(sessionState, query));
        }

        public Task<Page<T>> QueryPagedAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult(this.QueryPaged(sessionState, query));
        }

        public Task<int> CountAsync<T>(ISessionState sessionState, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult(this.Count(sessionState, query));
        }

        public Task<int> InsertAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult(this.Insert(sessionState, entities));
        }

        public Task<int> SaveAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult(this.Save(sessionState, entities));
        }

        public Task<int> DeleteAsync<T>(ISessionState sessionState, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult(this.Delete(sessionState, entities));
        }

        public Task<int> ExecuteAsync<T>(ISessionState sessionState, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult(this.Execute(sessionState, update, predicates));
        }

        public Task<int> ExecuteBulkDeleteAsync<T>(ISessionState sessionState, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult(this.ExecuteBulkDelete(sessionState, predicates));
        }

        private void AssertConfigured() {
            if (this.configuration == null) {
                throw new Exception("Configuration has not been set");
            }

            if (this.tables.Count == 0) {
                // create the in memory tables
                foreach (var map in this.configuration.Maps) {
                    var tableType = typeof(InMemoryTable<,>).MakeGenericType(map.Type, map.PrimaryKey.Type);
                    this.tables.Add(map.Type, Activator.CreateInstance(tableType, new object[] { this.configuration }));
                }
            }
        }
    }
}