namespace TopHat {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class FetchMany<TOriginal, TQueried> : IFetchMany<TOriginal, TQueried> {
        private readonly SelectQuery<TOriginal> query;

        private readonly Expression<Func<TOriginal, IEnumerable<TQueried>>> selector;

        public FetchMany(Expression<Func<TOriginal, IEnumerable<TQueried>>> selector, SelectQuery<TOriginal> query) {
            this.selector = selector;
            this.query = query;
        }

        public ISelectQuery<TOriginal> ThenFetch<TResult>(Expression<Func<TQueried, TResult>> selector) {
            if (this.query.CollectionFetches != null) {
                throw new Exception("You can only pre-fetch one collection");
            }

            this.query.CollectionFetches = new Tuple<Expression, Expression>(this.selector, selector);
            return this.query;
        }
    }
}