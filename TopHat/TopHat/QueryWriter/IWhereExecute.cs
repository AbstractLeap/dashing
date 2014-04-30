using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TopHat
{
    /// <summary>
    /// This interface is necessary for Delete, Update clauses
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWhereExecute<T>
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