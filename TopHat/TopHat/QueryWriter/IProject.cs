using System;
using System.Linq.Expressions;

namespace TopHat
{
    public interface IProject<T> : IFetch<T>
    {
        IFetch<T> Project(Expression<Func<T, dynamic>> selectExpression);

        IFetch<T> FetchAllProperties();
    }
}