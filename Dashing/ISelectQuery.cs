namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISelectQuery<T> : IEnumerableSelectQuery<T> where T : class, new() {
        /// <summary>
        ///     The select.
        /// </summary>
        /// <param name="projection">
        ///     The projection.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        IProjectedSelectQuery<T, TProjection> Select<TProjection>(Expression<Func<T, TProjection>> projection);

        /// <summary>
        ///     The include all.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> IncludeAll();

        /// <summary>
        ///     The include.
        /// </summary>
        /// <param name="includeExpression">
        ///     The include expression.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> Include<TResult>(Expression<Func<T, TResult>> includeExpression);

        /// <summary>
        ///     The exclude.
        /// </summary>
        /// <param name="excludeExpression">
        ///     The exclude expression.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> Exclude<TResult>(Expression<Func<T, TResult>> excludeExpression);

        IFetchMany<T, TResult> FetchMany<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector);

        /// <summary>
        ///     The fetch.
        /// </summary>
        /// <param name="selector">
        ///     The selector.
        /// </param>
        /// <typeparam name="TFetch">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> Fetch<TFetch>(Expression<Func<T, TFetch>> selector);

        /// <summary>
        ///     The for update.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> ForUpdate(bool skipLocked = false);

        /// <summary>
        ///     The skip.
        /// </summary>
        /// <param name="skip">
        ///     The skip.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> Skip(int skip);

        /// <summary>
        ///     The take.
        /// </summary>
        /// <param name="take">
        ///     The take.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> Take(int take);

        /// <summary>
        ///     The where.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate.
        /// </param>
        /// <returns>
        ///     The <see cref="QueryBase{T}" />.
        /// </returns>
        ISelectQuery<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        ///     The order by.
        /// </summary>
        /// <param name="keySelector">
        ///     The key selector.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> OrderBy<TResult>(Expression<Func<T, TResult>> keySelector);

        /// <summary>
        ///     The order by descending.
        /// </summary>
        /// <param name="keySelector">
        ///     The key selector.
        /// </param>
        /// <typeparam name="TResult">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> OrderByDescending<TResult>(Expression<Func<T, TResult>> keySelector);
        
        T First(Expression<Func<T, bool>> predicate);

        T FirstOrDefault(Expression<Func<T, bool>> predicate);

        T Single(Expression<Func<T, bool>> predicate);

        T SingleOrDefault(Expression<Func<T, bool>> predicate);

        T Last(Expression<Func<T, bool>> predicate);

        T LastOrDefault(Expression<Func<T, bool>> predicate);

        int Count();

        int Count(Expression<Func<T, bool>> predicate);

        bool Any();

        bool Any(Expression<Func<T, bool>> predicate);

        Task<T> FirstAsync(Expression<Func<T, bool>> predicate);

        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<T> SingleAsync(Expression<Func<T, bool>> predicate);

        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<T> LastAsync(Expression<Func<T, bool>> predicate);

        Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<int> CountAsync();

        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        Task<bool> AnyAsync();

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}