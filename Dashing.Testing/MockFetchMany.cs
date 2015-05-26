namespace Dashing.Testing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class MockFetchMany<TOriginal, TQueried> : IFetchMany<TOriginal, TQueried> {
        private KeyValuePair<Expression, Stack<Expression>> fetchExpression;

        private readonly MockSelectQuery<TOriginal> query;

        public MockFetchMany(Expression<Func<TOriginal, IEnumerable<TQueried>>> selector, MockSelectQuery<TOriginal> query) {
            this.query = query;
            this.fetchExpression = new KeyValuePair<Expression, Stack<Expression>>(selector, new Stack<Expression>());
            this.query.CollectionFetches.Add(this.fetchExpression);
        }

        public MockFetchMany(KeyValuePair<Expression, Stack<Expression>> currentFetchExpression, MockSelectQuery<TOriginal> query) {
            this.fetchExpression = currentFetchExpression;
            this.query = query;
        }

        public ISelectQuery<TOriginal> ThenFetch<TResult>(Expression<Func<TQueried, TResult>> selector) {
            this.fetchExpression.Value.Push(selector);
            return this.query;
        }

        public IFetchMany<TOriginal, TResult> ThenFetchMany<TResult>(Expression<Func<TQueried, IEnumerable<TResult>>> selector) {
            this.fetchExpression.Value.Push(selector);
            return new MockFetchMany<TOriginal, TResult>(this.fetchExpression, this.query);
        }
    }
}