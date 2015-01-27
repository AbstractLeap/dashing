namespace Dashing {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISelectQuery<T> : IEnumerable<T> {
        /// <summary>
        ///     The select.
        /// </summary>
        /// <param name="projection">
        ///     The projection.
        /// </param>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> Select(Expression<Func<T, object>> projection);

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
        ISelectQuery<T> ForUpdate();

        /// <summary>
        ///     The as tracked.
        /// </summary>
        /// <returns>
        ///     The <see cref="SelectQuery{T}" />.
        /// </returns>
        ISelectQuery<T> AsTracked();

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

        T First();

        T First(Expression<Func<T, bool>> predicate);

        T FirstOrDefault();

        T FirstOrDefault(Expression<Func<T, bool>> predicate);

        T Single();

        T Single(Expression<Func<T, bool>> predicate);

        T SingleOrDefault();

        T SingleOrDefault(Expression<Func<T, bool>> predicate);

        T Last();

        T Last(Expression<Func<T, bool>> predicate);

        T LastOrDefault();

        T LastOrDefault(Expression<Func<T, bool>> predicate);

        int Count();

        int Count(Expression<Func<T, bool>> predicate);
        
        Page<T> AsPaged(int skip, int take);

        bool Any();

        bool Any(Expression<Func<T, bool>> predicate);

        Task<IList<T>> ToListAsync();

        Task<T[]> ToArrayAsync();

        Task<T> FirstAsync();

        Task<T> FirstAsync(Expression<Func<T, bool>> predicate);

        Task<T> FirstOrDefaultAsync();

        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<T> SingleAsync();

        Task<T> SingleAsync(Expression<Func<T, bool>> predicate);

        Task<T> SingleOrDefaultAsync();

        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<T> LastAsync();

        Task<T> LastAsync(Expression<Func<T, bool>> predicate);

        Task<T> LastOrDefaultAsync();

        Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate);

        Task<int> CountAsync();

        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        Task<Page<T>> AsPagedAsync(int skip, int take);

        Task<bool> AnyAsync();

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    }
}