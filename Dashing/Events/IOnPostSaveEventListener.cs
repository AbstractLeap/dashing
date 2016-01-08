namespace Dashing.Events {
    public interface IOnPostSaveEventListener : IEventListener {
        void OnPostSave(object entity, ISession session);
    }
}