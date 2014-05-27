namespace TopHat {
    using System;
    using System.Linq.Expressions;

    public interface IQuery<T> {
        /// <summary>
        ///     The where.
        /// </summary>
        /// <param name="predicate">
        ///     The predicate.
        /// </param>
        /// <returns>
        ///     The <see cref="QueryBase{T}" />.
        /// </returns>
        QueryBase<T> Where(Expression<Func<T, bool>> predicate);
    }
}