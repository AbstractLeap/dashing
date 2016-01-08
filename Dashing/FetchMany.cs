namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class FetchMany<TOriginal, TQueried> : IFetchMany<TOriginal, TQueried>
        where TOriginal : class, new() {
        private readonly SelectQuery<TOriginal> query;

        private KeyValuePair<Expression, List<Expression>> fetchExpression;

        public FetchMany(Expression<Func<TOriginal, IEnumerable<TQueried>>> selector, SelectQuery<TOriginal> query) {
            this.query = query;
            this.fetchExpression = new KeyValuePair<Expression, List<Expression>>(selector, new List<Expression>());
            this.query.CollectionFetches.Add(this.fetchExpression);
        }

        public FetchMany(KeyValuePair<Expression, List<Expression>> currentFetchExpression, SelectQuery<TOriginal> query) {
            this.fetchExpression = currentFetchExpression;
            this.query = query;
        }

        public ISelectQuery<TOriginal> ThenFetch<TResult>(Expression<Func<TQueried, TResult>> selector) {
            this.fetchExpression.Value.Add(selector);
            return this.query;
        }

        public IFetchMany<TOriginal, TResult> ThenFetchMany<TResult>(Expression<Func<TQueried, IEnumerable<TResult>>> selector) {
            this.fetchExpression.Value.Add(selector);
            return new FetchMany<TOriginal, TResult>(this.fetchExpression, this.query);
        }
    }
}