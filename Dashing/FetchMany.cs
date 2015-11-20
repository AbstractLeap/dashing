namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class FetchMany<TOriginal, TQueried> : IFetchMany<TOriginal, TQueried> {
        private readonly SelectQuery<TOriginal> query;

        private KeyValuePair<Expression, Stack<Expression>> fetchExpression;

        public FetchMany(Expression<Func<TOriginal, IEnumerable<TQueried>>> selector, SelectQuery<TOriginal> query) {
            this.query = query;
            this.fetchExpression = new KeyValuePair<Expression, Stack<Expression>>(selector, new Stack<Expression>());
            this.query.CollectionFetches.Add(this.fetchExpression);
        }

        public FetchMany(KeyValuePair<Expression, Stack<Expression>> currentFetchExpression, SelectQuery<TOriginal> query) {
            this.fetchExpression = currentFetchExpression;
            this.query = query;
        }

        public ISelectQuery<TOriginal> ThenFetch<TResult>(Expression<Func<TQueried, TResult>> selector) {
            this.fetchExpression.Value.Push(selector);
            return this.query;
        }

        public IFetchMany<TOriginal, TResult> ThenFetchMany<TResult>(Expression<Func<TQueried, IEnumerable<TResult>>> selector) {
            this.fetchExpression.Value.Push(selector);
            return new FetchMany<TOriginal, TResult>(this.fetchExpression, this.query);
        }
    }
}