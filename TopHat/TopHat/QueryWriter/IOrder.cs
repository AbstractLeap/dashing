using System;
using System.Linq.Expressions;

namespace TopHat
{
    public interface IOrder<T> : IQuery<T>
    {
        IOrder<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector);

        IOrder<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector);

        IOrder<T> OrderBy(string condition);

        IOrder<T> OrderByDescending(string condition);
    }
}