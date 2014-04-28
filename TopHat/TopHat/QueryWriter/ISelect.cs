using System;
using System.Linq.Expressions;

namespace TopHat
{
    public interface ISelect<T> : IFetch<T>
    {
        /// <summary>
        /// Enables projection of results in to another expression
        /// </summary>
        /// <param name="selectExpression"></param>
        /// <returns></returns>
        IFetch<T> Select(Expression<Func<T, dynamic>> selectExpression);

        /// <summary>
        /// Include all columns in the result set
        /// </summary>
        /// <returns></returns>
        ISelect<T> IncludeAll();

        /// <summary>
        /// Enables inclusion of non-eager columns
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="includeExpression"></param>
        /// <returns></returns>
        ISelect<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression);

        /// <summary>
        /// Exclude a particular column from the result set
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="excludeExpression"></param>
        /// <returns></returns>
        ISelect<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression);
    }
}