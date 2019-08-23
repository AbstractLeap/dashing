namespace Dashing {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEnumerableSelectQuery<T> : IEnumerable<T> {
        T First();

        T FirstOrDefault();

        T Single();

        T SingleOrDefault();

        T Last();

        T LastOrDefault();

        Page<T> AsPaged(int skip, int take);

        Task<IList<T>> ToListAsync();

        Task<T[]> ToArrayAsync();

        Task<T> FirstAsync();

        Task<T> FirstOrDefaultAsync();

        Task<T> SingleAsync();

        Task<T> SingleOrDefaultAsync();

        Task<T> LastAsync();

        Task<T> LastOrDefaultAsync();

        Task<Page<T>> AsPagedAsync(int skip, int take);
    }
}