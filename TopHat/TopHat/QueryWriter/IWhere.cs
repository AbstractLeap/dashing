using System;
using System.Linq.Expressions;

namespace TopHat
{
    public interface IWhere<T> : IOrder<T>
    {
        /// <summary>
        /// Add a where clause to the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <remarks>If this is a delete or update clause it will execute the query</remarks>
        IWhere<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add a where clause to the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <remarks>If this is a delete or update clause it will execute the query</remarks>
        IWhere<T> Where(string condition);

        /// <summary>
        /// Add a where clause to the query
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <remarks>If this is a delete or update clause it will execute the query</remarks>
        IWhere<T> Where(string condition, params dynamic[] parameters);
    }
}