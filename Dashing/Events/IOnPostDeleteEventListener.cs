namespace Dashing.Events {
    public interface IOnPostDeleteEventListener : IEventListener {
        void OnPostDelete(object entity, ISession session);
    }
}