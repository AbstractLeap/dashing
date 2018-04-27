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
    using Dashing.SqlBuilder;

    public class InMemoryEngine : IEngine {
        private Dictionary<Type, object> tables;

        private IConfiguration configuration;

        public InMemoryEngine(IConfiguration configuration) {
            this.configuration = configuration;
            this.tables = new Dictionary<Type, object>();
        }

        public IConfiguration Configuration {
            get {
                return this.configuration;
            }
        }

        public InMemoryTable<TEntity, TPrimaryKey> GetTable<TEntity, TPrimaryKey>() where TEntity : class, new() {
            if (!tables.ContainsKey(typeof(TEntity))) {
                throw new Exception(string.Format("The type {0} is not part of the configuration", typeof(TEntity).FullName));
            }

            return (InMemoryTable<TEntity, TPrimaryKey>)this.tables[typeof(TEntity)];
        }

        public T Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new() {
            this.AssertConfigured();
            return this.GetTable<T, TPrimaryKey>().Get(id);
        }

        public IEnumerable<T> Query<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            this.AssertConfigured();
            return ids.Select<TPrimaryKey, T>(i => this.GetTable<T, TPrimaryKey>().Get(i));
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

        public Page<T> QueryPaged<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            return new Page<T> {
                Items = this.Query<T>(connection, transaction, query),
                Skipped = query.SkipN,
                Taken = query.TakeN,
                TotalResults = this.Count(connection, transaction, query)
            };
        }

        public IEnumerable<T> Query<T>(IDbConnection connection, IDbTransaction transaction, BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression) {
            throw new NotImplementedException();
        }

        public int Count<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            this.AssertConfigured();
            return Enumerable.Count<T>(this.Query<T>(connection, transaction, query));
        }

        public int Insert<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var insertMethod = typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap<T>().PrimaryKey.Type).GetMethod("Insert");
            var results = 0;
            foreach (var entity in entities) {
                results += (int)insertMethod.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Save<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var updateMethod = typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap<T>().PrimaryKey.Type).GetMethod("Update");
            var results = 0;
            foreach (var entity in entities) {
                results += (int)updateMethod.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Delete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            this.AssertConfigured();
            var table = this.tables[typeof(T)];
            var deleteMethod = typeof(InMemoryTable<,>).MakeGenericType(typeof(T), this.Configuration.GetMap<T>().PrimaryKey.Type).GetMethod("Delete");
            var results = 0;
            foreach (var entity in entities) {
                results += (int)deleteMethod.Invoke(table, new[] { entity });
            }

            return results;
        }

        public int Execute<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.AssertConfigured();
            var query = new SelectQuery<T>(new NonExecutingSelectQueryExecutor());
            if (predicates != null) {
                foreach (var predicate in predicates) {
                    query.Where(predicate);
                }
            }

            var entitiesToUpdate = Enumerable.ToList<T>(this.Query<T>(connection, transaction, query));
            foreach (var entity in entitiesToUpdate) {
                update(entity);
            }

            return this.Save(connection, transaction, entitiesToUpdate);
        }

        public int ExecuteBulkDelete<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            this.AssertConfigured();
            var query = new SelectQuery<T>(new NonExecutingSelectQueryExecutor());
            if (predicates != null) {
                foreach (var predicate in predicates) {
                    query.Where(predicate);
                }
            }

            var entitiesToDelete = Enumerable.ToList<T>(this.Query<T>(connection, transaction, query));
            return this.Delete(connection, transaction, entitiesToDelete);
        }

        public Task<T> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, TPrimaryKey id) where T : class, new() {
            return Task.FromResult<T>(this.Query<T, TPrimaryKey>(connection, transaction, id));
        }

        public Task<IEnumerable<T>> QueryAsync<T, TPrimaryKey>(IDbConnection connection, IDbTransaction transaction, IEnumerable<TPrimaryKey> ids) where T : class, new() {
            return Task.FromResult<IEnumerable<T>>(this.Query<T, TPrimaryKey>(connection, transaction, ids));
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult<IEnumerable<T>>(this.Query<T>(connection, transaction, query));
        }

        public Task<Page<T>> QueryPagedAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult<Page<T>>(this.QueryPaged(connection, transaction, query));
        }

        public Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, IDbTransaction transaction, BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression) {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync<T>(IDbConnection connection, IDbTransaction transaction, SelectQuery<T> query) where T : class, new() {
            return Task.FromResult<int>(this.Count(connection, transaction, query));
        }

        public Task<int> InsertAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult<int>(this.Insert(connection, transaction, entities));
        }

        public Task<int> SaveAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult<int>(this.Save(connection, transaction, entities));
        }

        public Task<int> DeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<T> entities) where T : class, new() {
            return Task.FromResult<int>(this.Delete(connection, transaction, entities));
        }

        public Task<int> ExecuteAsync<T>(IDbConnection connection, IDbTransaction transaction, Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult<int>(this.Execute(connection, transaction, update, predicates));
        }

        public Task<int> ExecuteBulkDeleteAsync<T>(IDbConnection connection, IDbTransaction transaction, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult<int>(this.ExecuteBulkDelete(connection, transaction, predicates));
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