namespace Dashing.Events {
    public interface IOnPreDeleteEventListener : IEventListener {
        void OnPreDelete(object entity, ISession session);
    }
}