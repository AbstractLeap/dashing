namespace Dashing.Testing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Dashing.Configuration;

    public class MockSession : ISession {
        private int insertId = 1000;

        private readonly IDictionary<Type, IList> testLists;

        public MockSession(IDictionary<Type, IList> testLists)
            : this() {
            // we shallow copy so that we dont update the original list
            foreach (var testList in testLists) {
                // create an array of values
                var array = Array.CreateInstance(testList.Key, testList.Value.Count);
                testList.Value.CopyTo(array, 0);

                // now call the generic method
                this.GetType().GetMethod("AddTestEntities").MakeGenericMethod(testList.Key).Invoke(this, new object[] { array });
            }
        }

        public MockSession() {
            this.testLists = new Dictionary<Type, IList>();
            this.Dapper = new MockDapper();
            this.Queries = new Dictionary<Type, IList>();
            this.Inserts = new Dictionary<Type, IList>();
            this.Saves = new Dictionary<Type, IList>();
            this.Deletes = new Dictionary<Type, IList>();
            this.BulkUpdates = new Dictionary<Type, IList>();
            this.Configuration = new MockConfiguration();
        }

        public IDictionary<Type, IList> Inserts { get; set; }

        public IDictionary<Type, IList> Saves { get; set; }

        public Dictionary<Type, IList> Deletes { get; set; }

        public IDictionary<Type, IList> Queries { get; set; }

        public IDictionary<Type, IList> BulkUpdates { get; set; }

        public IConfiguration Configuration { get; set; }

        public IDapper Dapper { get; private set; }

        public void Dispose() {}

        public void Complete() {}

        public void Reject() {}

        public T Get<T, TPrimaryKey>(TPrimaryKey id) {
            return this.GetQuery<T>().Where(this.GetPrimaryKeyWhereClause<T, TPrimaryKey>(id)).Single();
        }

        public IEnumerable<T> Get<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return this.GetQuery<T>().Where(this.GetPrimaryKeysWhereClause<T, TPrimaryKey>(ids)).ToArray();
        }

        public ISelectQuery<T> Query<T>() {
            if (!this.Queries.ContainsKey(typeof(T))) {
                this.Queries.Add(typeof(T), new List<ISelectQuery<T>>());
            }

            var query = this.GetQuery<T>();
            this.Queries[typeof(T)].Add(query);
            return query;
        }

        public int Insert<T>(IEnumerable<T> entities) {
            // add to inserts list
            if (!this.Inserts.ContainsKey(typeof(T))) {
                this.Inserts.Add(typeof(T), new List<T>());
            }

            var list = this.Inserts[typeof(T)] as List<T>;
            list.AddRange(entities);

            // add to test list as well
            var testList = this.GetOrInitTestList<T>();
            foreach (var entity in entities) {
                testList.Add(entity);
                entity.GetType().GetProperty(entity.GetType().Name + "Id").SetValue(entity, ++this.insertId);
            }

            return entities.Count();
        }

        public int Save<T>(IEnumerable<T> entities) {
            // add to saves list
            if (!this.Saves.ContainsKey(typeof(T))) {
                this.Saves.Add(typeof(T), new List<T>());
            }

            var list = this.Saves[typeof(T)] as List<T>;
            list.AddRange(entities);
            return entities.Count();
        }

        public int Update<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            if (!this.BulkUpdates.ContainsKey(typeof(T))) {
                this.BulkUpdates.Add(typeof(T), new List<Tuple<Action<T>, IEnumerable<Expression<Func<T, bool>>>>>());
            }

            var list = this.BulkUpdates[typeof(T)] as IList<Tuple<Action<T>, IEnumerable<Expression<Func<T, bool>>>>>;
            list.Add(Tuple.Create(update, predicates));

            // perform the update on the test list
            var testList = this.GetOrInitTestList<T>();
            var pred = this.CombineExpressions(predicates).Compile();
            var updates = 0;
            foreach (var entity in testList) {
                if (new[] { entity }.Any(pred)) {
                    update(entity);
                    updates++;
                }
            }

            return updates;
        }

        public int Delete<T>(IEnumerable<T> entities) {
            var deleteList = this.GetOrInitDeleteList<T>();
            var testList = this.GetOrInitTestList<T>();
            var i = 0;

            foreach (var entity in entities.Where(entity => testList.Remove(entity))) {
                deleteList.Add(entity);
                ++i;
            }

            return i;
        }

        public int Delete<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            return this.Delete(this.GetOrInitTestList<T>().Where(this.CombineExpressions(predicates).Compile()).ToArray());
        }

        public int UpdateAll<T>(Action<T> update) where T : class, new() {
            if (!this.BulkUpdates.ContainsKey(typeof(T))) {
                this.BulkUpdates.Add(typeof(T), new List<Tuple<Action<T>, IEnumerable<Expression<Func<T, bool>>>>>());
            }

            var list = this.BulkUpdates[typeof(T)] as IList<Tuple<Action<T>, IEnumerable<Expression<Func<T, bool>>>>>;
            list.Add(Tuple.Create(update, new List<Expression<Func<T, bool>>>()));
            
            var entities = this.GetOrInitTestList<T>();
            var updates = 0;
            foreach(var entity in entities) {
                update(entity);
                updates++;
            }
            
            return updates;
        }

        public int DeleteAll<T>() {
            return this.Delete(this.GetOrInitTestList<T>());
        }

        public Task<T> GetAsync<T, TPrimaryKey>(TPrimaryKey id) {
            return Task.FromResult(this.Get<T, TPrimaryKey>(id));
        }

        public Task<IEnumerable<T>> GetAsync<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            return Task.FromResult(this.Get<T, TPrimaryKey>(ids));
        }

        public Task<int> InsertAsync<T>(IEnumerable<T> entities) {
            return Task.FromResult(this.Insert(entities));
        }

        public Task<int> SaveAsync<T>(IEnumerable<T> entities) {
            return Task.FromResult(this.Save(entities));
        }

        public Task<int> UpdateAsync<T>(Action<T> update, IEnumerable<Expression<Func<T, bool>>> predicates) where T : class, new() {
            return Task.FromResult(this.Update(update, predicates));
        }

        public Task<int> DeleteAsync<T>(IEnumerable<T> entities) {
            return Task.FromResult(this.Delete(entities));
        }

        public Task<int> DeleteAsync<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            return Task.FromResult(this.Delete(predicates));
        }

        public Task<int> UpdateAllAsync<T>(Action<T> update) where T : class, new() {
            return Task.FromResult(this.UpdateAll(update));
        }

        public Task<int> DeleteAllAsync<T>() {
            return Task.FromResult(this.DeleteAll());
        }

        public void AddTestEntities<T>(params T[] entities) {
            var testList = this.GetOrInitTestList<T>();
            foreach (var entity in entities) {
                testList.Add(entity);
            }
        }

        private Expression<Func<T, bool>> GetPrimaryKeyWhereClause<T, TPrimaryKey>(TPrimaryKey id) {
            var param = Expression.Parameter(typeof(T));
            var compare = Expression.Equal(Expression.Property(param, typeof(T).Name + "Id"), Expression.Constant(id));
            return Expression.Lambda<Func<T, bool>>(compare, param);
        }

        private Expression<Func<T, bool>> GetPrimaryKeysWhereClause<T, TPrimaryKey>(IEnumerable<TPrimaryKey> ids) {
            var param = Expression.Parameter(typeof(T));
            var compare = Expression.Call(
                null,
                typeof(Enumerable).GetMethods().First(m => m.Name == "Contains" && m.GetParameters().Count() == 2).MakeGenericMethod(typeof(TPrimaryKey)),
                Expression.Constant(ids),
                Expression.Property(param, typeof(T).Name + "Id"));
            return Expression.Lambda<Func<T, bool>>(compare, param);
        }

        private ISelectQuery<T> GetQuery<T>() {
            var testList = this.GetOrInitTestList<T>();
            return new MockSelectQuery<T>(testList);
        }

        private IList<T> GetOrInitTestList<T>() {
            if (!this.testLists.ContainsKey(typeof(T))) {
                this.testLists.Add(typeof(T), new List<T>());
            }

            return this.testLists[typeof(T)] as IList<T>;
        }

        private Expression<Func<T, bool>> CombineExpressions<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
            if (predicates.Count() == 1) {
                return predicates.First();
            }

            if (!predicates.Any()) {
                throw new InvalidOperationException();
            }

            var expr = Expression.AndAlso(predicates.First(), predicates.ElementAt(1));
            for (var i = 2; i < predicates.Count(); i++) {
                expr = Expression.AndAlso(expr, predicates.ElementAt(i));
            }

            return Expression.Lambda<Func<T, bool>>(expr, predicates.First().Parameters.First());
        }

        private IList GetOrInitDeleteList<T>() {
            if (!this.Deletes.ContainsKey(typeof(T))) {
                this.Deletes[typeof(T)] = new List<T>();
            }

            return this.Deletes[typeof(T)];
        }
    }
}