namespace Dashing {
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IProjectedSelectQueryExecutor : ISelectQueryExecutor {
        IEnumerable<TProjection> Query<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new();

        Page<TProjection> QueryPaged<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new();

        Task<IEnumerable<TProjection>> QueryAsync<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new();

        Task<Page<TProjection>> QueryPagedAsync<TBase, TProjection>(ProjectedSelectQuery<TBase, TProjection> query)
            where TBase : class, new();
    }
}