namespace Dashing.SqlBuilder {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISqlQuerySelection<TResult> : IEnumerable<TResult> {
        Task<IEnumerable<TResult>> EnumerateAsync();
    }
}