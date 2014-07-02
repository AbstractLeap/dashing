namespace Dashing {
    using System;
    using System.Linq.Expressions;

    public interface IFetchMany<TOriginal, TQueried> {
        ISelectQuery<TOriginal> ThenFetch<TResult>(Expression<Func<TQueried, TResult>> selector);
    }
}