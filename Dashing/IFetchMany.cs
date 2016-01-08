namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IFetchMany<TOriginal, TQueried>
        where TOriginal : class, new() {
        ISelectQuery<TOriginal> ThenFetch<TResult>(Expression<Func<TQueried, TResult>> selector);

        IFetchMany<TOriginal, TResult> ThenFetchMany<TResult>(Expression<Func<TQueried, IEnumerable<TResult>>> selector);
    }
}