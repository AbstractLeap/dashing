namespace Dashing {
    public interface IProjectedSelectQuery<TBase, TProjection> : IEnumerableSelectQuery<TProjection>
        where TBase : class, new() {

    }
}