using System;
using System.Linq.Expressions;

namespace TopHat
{
    public interface IWhere<T> : IOrder<T>
    {
        IWhere<T> Where(Expression<Func<T, bool>> predicate);

        IWhere<T> Where(string condition);

        IWhere<T> Where(string condition, params dynamic[] parameters);
    }
}