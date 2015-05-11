namespace Dashing.Testing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class MockSelectQuery<T> : ISelectQuery<T> {
        private IEnumerable<T> query;

        private readonly IList<T> testList;

        private readonly WhereClauseNullCheckRewriter whereClauseNullCheckRewriter;

        public MockSelectQuery(IList<T> testList) {
            this.testList = testList;
            this.query = testList;
            this.Fetches = new List<Expression>();
            this.CollectionFetches = new List<KeyValuePair<Expression, Stack<Expression>>>();
            this.WhereClauses = new List<Expression>();
            this.OrderClauses = new List<KeyValuePair<Expression, ListSortDirection>>();
            this.whereClauseNullCheckRewriter = new WhereClauseNullCheckRewriter();
        }

        public IList<Expression> Fetches { get; set; }

        public IList<KeyValuePair<Expression, Stack<Expression>>> CollectionFetches { get; private set; }

        public IList<Expression> WhereClauses { get; set; }

        public IList<KeyValuePair<Expression, ListSortDirection>> OrderClauses { get; set; }

        public bool IsForUpdate { get; set; }

        public bool IsTracked { get; set; }

        public int TakeN { get; set; }

        public IEnumerator<T> GetEnumerator() {
            return this.testList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public ISelectQuery<T> Select(Expression<Func<T, object>> projection) {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> IncludeAll() {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression) {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression) {
            throw new NotImplementedException();
        }

        public IFetchMany<T, TResult> FetchMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector) {
            return new MockFetchMany<T, TResult>(selector, this);
        }

        public ISelectQuery<T> Fetch<TFetch>(Expression<Func<T, TFetch>> selector) {
            this.Fetches.Add(selector);
            return this;
        }

        public ISelectQuery<T> ForUpdate() {
            this.IsForUpdate = true;
            return this;
        }

        public ISelectQuery<T> AsTracked() {
            this.IsTracked = true;
            return this;
        }

        public ISelectQuery<T> AsNonTracked() {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Skip(int skip) {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> Take(int take) {
            this.TakeN = take;
            this.query = this.query.Take(take);
            return this;
        }

        public ISelectQuery<T> Where(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            this.query = this.query.Where(this.whereClauseNullCheckRewriter.Rewrite(predicate).Compile());
            return this;
        }

        public ISelectQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Add(new KeyValuePair<Expression, ListSortDirection>(keySelector, ListSortDirection.Ascending));
            this.query = this.query.OrderBy(keySelector.Compile());
            return this;
        }

        public ISelectQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Add(new KeyValuePair<Expression, ListSortDirection>(keySelector, ListSortDirection.Descending));
            this.query = this.query.OrderByDescending(keySelector.Compile());
            return this;
        }

        public T First() {
            return this.query.First();
        }

        public T First(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.query.First(this.whereClauseNullCheckRewriter.Rewrite(predicate).Compile());
        }

        public T FirstOrDefault() {
            return this.query.FirstOrDefault();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.query.FirstOrDefault(this.whereClauseNullCheckRewriter.Rewrite(predicate).Compile());
        }

        public T Single() {
            return this.query.Single();
        }

        public T Single(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.Single(this.whereClauseNullCheckRewriter.Rewrite(predicate).Compile());
        }

        public T SingleOrDefault() {
            return this.query.SingleOrDefault();
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.query.SingleOrDefault(this.whereClauseNullCheckRewriter.Rewrite(predicate).Compile());
        }

        public T Last() {
            throw new NotImplementedException();
        }

        public T Last(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public T LastOrDefault() {
            throw new NotImplementedException();
        }

        public T LastOrDefault(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public int Count() {
            throw new NotImplementedException();
        }

        public int Count(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public Page<T> AsPaged(int skip, int take) {
            throw new NotImplementedException();
        }

        public bool Any() {
            throw new NotImplementedException();
        }

        public bool Any(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public Task<IList<T>> ToListAsync() {
            return Task.FromResult((IList<T>)this.query.ToList());
        }

        public Task<T[]> ToArrayAsync() {
            return Task.FromResult(this.query.ToArray());
        }

        public Task<T> FirstAsync() {
            return Task.FromResult(this.First());
        }

        public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.First(predicate));
        }

        public Task<T> FirstOrDefaultAsync() {
            return Task.FromResult(this.FirstOrDefault());
        }

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.FirstOrDefault(predicate));
        }

        public Task<T> SingleAsync() {
            return Task.FromResult(this.Single());
        }

        public Task<T> SingleAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.Single(predicate));
        }

        public Task<T> SingleOrDefaultAsync() {
            return Task.FromResult(this.SingleOrDefault());
        }

        public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.SingleOrDefault(predicate));
        }

        public Task<T> LastAsync() {
            throw new NotImplementedException();
        }

        public Task<T> LastAsync(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public Task<T> LastOrDefaultAsync() {
            throw new NotImplementedException();
        }

        public Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync() {
            throw new NotImplementedException();
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }

        public Task<Page<T>> AsPagedAsync(int skip, int take) {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync() {
            throw new NotImplementedException();
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) {
            throw new NotImplementedException();
        }
    }
}