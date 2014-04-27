using System.Collections.Generic;
using System.Threading.Tasks;

namespace TopHat
{
    public interface IQuery<T> : IEnumerable<T>
    {
        /// <summary>
        /// Locks the returned objects on the database to prevent selection and updating by other connections
        /// </summary>
        /// <returns></returns>
        /// <remarks>See https://dev.mysql.com/doc/refman/5.7/en/innodb-locking-reads.html </remarks>
        IQuery<T> ForUpdate();

        /// <summary>
        /// Skips the specified number of rows in returning the result set
        /// </summary>
        /// <param name="skip">Number of rows to skip</param>
        /// <returns></returns>
        IQuery<T> Skip(int skip);

        /// <summary>
        /// Returns the specified number of rows in the result set
        /// </summary>
        /// <param name="take">Number of rows to return</param>
        /// <returns></returns>
        IQuery<T> Take(int take);

        /// <summary>
        /// Fetches the results using an asynchronous database reader
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> Async();

        /// <summary>
        /// Returns the underlying query object for this query
        /// </summary>
        Query<T> Query { get; }
    }
}