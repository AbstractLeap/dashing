using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TopHat
{
    public interface IFetch<TOriginating> : IWhere<TOriginating>
    {
        IThenFetch<TOriginating, TFetch> Fetch<TFetch>(Expression<Func<TOriginating, TFetch>> relatedObjectSelector);

        IThenFetch<TOriginating, TFetch> FetchMany<TFetch>(Expression<Func<TOriginating, IEnumerable<TFetch>>> relatedObjectSelector);
    }

    public interface IThenFetch<TQueried, TFetch> : IFetch<TQueried>
    {
        IThenFetch<TQueried, TRelated> ThenFetch<TRelated>(Expression<Func<TFetch, TRelated>> relatedObjectSelector);
    }
}