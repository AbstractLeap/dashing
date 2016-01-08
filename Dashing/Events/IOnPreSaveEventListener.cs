namespace Dashing.Events {
    public interface IOnPreSaveEventListener : IEventListener {
        void OnPreSave(object entity, ISession session);
    }
}