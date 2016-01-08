namespace Dashing.Events {
    public interface IOnPostInsertEventListener : IEventListener {
        void OnPostInsert(object entity, ISession session);
    }
}