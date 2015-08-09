// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Dashing.Testing {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class MockSelectQuery<T> : ISelectQuery<T> {
        private readonly IList<T> allRecords;

        private readonly WhereClauseNullCheckRewriter whereClauseNullCheckRewriter;

        public MockSelectQuery(IList<T> allRecords) {
            this.allRecords = allRecords;
            this.Fetches = new List<Expression>();
            this.CollectionFetches = new List<KeyValuePair<Expression, Stack<Expression>>>();
            this.WhereClauses = new List<Expression<Func<T, bool>>>();
            this.OrderClauses = new List<Tuple<Expression, ListSortDirection, Func<IEnumerable<T>, IEnumerable<T>>>>();
            this.whereClauseNullCheckRewriter = new WhereClauseNullCheckRewriter();
        }

        public IList<Expression> Includes { get; set; }

        public IList<Expression> Excludes { get; set; }
        
        public IList<Expression> Fetches { get; set; }

        public IList<KeyValuePair<Expression, Stack<Expression>>> CollectionFetches { get; private set; }

        public IList<Expression<Func<T, bool>>> WhereClauses { get; set; }

        public IList<Tuple<Expression, ListSortDirection, Func<IEnumerable<T>, IEnumerable<T>>>> OrderClauses { get; set; }

        public bool IsForUpdate { get; set; }

        public bool IsIncludingAll { get; set; }

        public int? TakeN { get; set; }
        
        public int? SkipN { get; set; }

        private IEnumerable<T> Results {
            get {
                var q = this.allRecords.ToArray() as IEnumerable<T>;

                foreach (var predicate in this.WhereClauses) {
                    q = q.Where(this.whereClauseNullCheckRewriter.Rewrite(predicate).Compile());
                }

                foreach (var order in this.OrderClauses) {
                    q = order.Item3(q);
                }

                if (this.SkipN.HasValue) {
                    q = q.Skip(this.SkipN.Value);
                }
                
                if (this.TakeN.HasValue) {
                    q = q.Take(this.TakeN.Value);
                }

                return q;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return this.Results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public ISelectQuery<T> Select(Expression<Func<T, object>> projection) {
            throw new NotImplementedException();
        }

        public ISelectQuery<T> IncludeAll() {
            this.IsIncludingAll = true;
            return this;
        }
        
        public ISelectQuery<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression) {
            this.Includes.Add(includeExpression);
            return this;
        }

        public ISelectQuery<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression) {
            this.Excludes.Add(excludeExpression);
            return this;
        }

        public IFetchMany<T, TResult> FetchMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector) {
            // the MockFetchMany updates CollectionFetches for us
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

        public ISelectQuery<T> Skip(int skip) {
            this.SkipN = skip;
            return this;
        }

        public ISelectQuery<T> Take(int take) {
            this.TakeN = take;
            return this;
        }

        public ISelectQuery<T> Where(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this;
        }

        public ISelectQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Add(Tuple.Create<Expression, ListSortDirection, Func<IEnumerable<T>, IEnumerable<T>>>(keySelector, ListSortDirection.Ascending, q => q.OrderBy(keySelector.Compile())));
            return this;
        }

        public ISelectQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector) {
            this.OrderClauses.Add(Tuple.Create<Expression, ListSortDirection, Func<IEnumerable<T>, IEnumerable<T>>>(keySelector, ListSortDirection.Descending, q => q.OrderByDescending(keySelector.Compile())));
            return this;
        }

        public T First() {
            return this.Results.First();
        }

        public T First(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.First();
        }

        public T FirstOrDefault() {
            return this.Results.FirstOrDefault();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.FirstOrDefault();
        }

        public T Single() {
            return this.Results.Single();
        }

        public T Single(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.Single();
        }

        public T SingleOrDefault() {
            return this.Results.SingleOrDefault();
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.SingleOrDefault();
        }

        public T Last() {
            return this.Results.Last();
        }

        public T Last(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.Last();
        }

        public T LastOrDefault() {
            return this.Results.LastOrDefault();
        }

        public T LastOrDefault(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.LastOrDefault();
        }

        public int Count() {
            return this.Results.Count();
        }

        public int Count(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.Count();
        }

        public Page<T> AsPaged(int skip, int take) {
            this.SkipN = this.TakeN = null;
            var totalResults = this.Count();
            this.SkipN = skip;
            this.TakeN = take;

            return new Page<T> {
                Items = this.Results.ToArray(),
                Skipped = skip,
                Taken = take,
                TotalResults = totalResults
            };
        }

        public bool Any() {
            return this.Results.Any();
        }

        public bool Any(Expression<Func<T, bool>> predicate) {
            this.WhereClauses.Add(predicate);
            return this.Any();
        }

        public Task<IList<T>> ToListAsync() {
            return Task.FromResult((IList<T>)this.Results.ToList());
        }

        public Task<T[]> ToArrayAsync() {
            return Task.FromResult(this.Results.ToArray());
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
            return Task.FromResult(this.Last());
        }

        public Task<T> LastAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.Last(predicate));
        }

        public Task<T> LastOrDefaultAsync() {
            return Task.FromResult(this.LastOrDefault());
        }

        public Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.LastOrDefault(predicate));
        }

        public Task<int> CountAsync() {
            return Task.FromResult(this.Count());
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.Count(predicate));
        }

        public Task<Page<T>> AsPagedAsync(int skip, int take) {
            return Task.FromResult(this.AsPaged(skip, take));
        }

        public Task<bool> AnyAsync() {
            return Task.FromResult(this.Any());
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) {
            return Task.FromResult(this.Any(predicate));
        }
    }
}