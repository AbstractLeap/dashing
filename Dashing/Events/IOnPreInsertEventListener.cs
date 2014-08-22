namespace Dashing.Events {
    public interface IOnPreInsertEventListener : IEventListener {
        void OnPreInsert(object entity, ISession session);
    }
}
