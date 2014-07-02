namespace Dashing.Configuration {
    public interface IMap<T> : IMap {
        object GetPrimaryKeyValue(T entity);

        void SetPrimaryKeyValue(T entity, object value);

        object GetColumnValue(T entity, IColumn column);
    }
}