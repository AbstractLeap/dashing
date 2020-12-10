namespace Dashing.Events {
    using System.Collections.Generic;

    public class EventHandlers {
        public EventHandlers(IEnumerable<IEventListener> listeners) {
            this.PreInsertListeners = new List<IOnPreInsertEventListener>();
            this.PreSaveListeners = new List<IOnPreSaveEventListener>();
            this.PreDeleteListeners = new List<IOnPreDeleteEventListener>();
            this.PostInsertListeners = new List<IOnPostInsertEventListener>();
            this.PostSaveListeners = new List<IOnPostSaveEventListener>();
            this.PostDeleteListeners = new List<IOnPostDeleteEventListener>();
            this.UpdateListeners(listeners);
        }

        public IList<IOnPreInsertEventListener> PreInsertListeners { get; private set; }

        public IList<IOnPreSaveEventListener> PreSaveListeners { get; private set; }

        public IList<IOnPreDeleteEventListener> PreDeleteListeners { get; private set; }

        public IList<IOnPostInsertEventListener> PostInsertListeners { get; private set; }

        public IList<IOnPostSaveEventListener> PostSaveListeners { get; private set; }

        public IList<IOnPostDeleteEventListener> PostDeleteListeners { get; private set; }

        internal void Invalidate(IEnumerable<IEventListener> listeners) {
            this.UpdateListeners(listeners);
        }

        private void UpdateListeners(IEnumerable<IEventListener> listeners) {
            foreach (var listener in listeners) {
                if (listener is IOnPreInsertEventListener preInsertEventListener && !this.PreInsertListeners.Contains(preInsertEventListener)) {
                    this.PreInsertListeners.Add(preInsertEventListener);
                }

                if (listener is IOnPreSaveEventListener preSaveEventListener && !this.PreSaveListeners.Contains(preSaveEventListener)) {
                    this.PreSaveListeners.Add(preSaveEventListener);
                }

                if (listener is IOnPreDeleteEventListener preDeleteEventListener && !this.PreDeleteListeners.Contains(preDeleteEventListener)) {
                    this.PreDeleteListeners.Add(preDeleteEventListener);
                }

                if (listener is IOnPostInsertEventListener postInsertEventListener && !this.PostInsertListeners.Contains(postInsertEventListener)) {
                    this.PostInsertListeners.Add(postInsertEventListener);
                }

                if (listener is IOnPostSaveEventListener postSaveEventListener && !this.PostSaveListeners.Contains(postSaveEventListener)) {
                    this.PostSaveListeners.Add(postSaveEventListener);
                }

                if (listener is IOnPostDeleteEventListener postDeleteEventListener && !this.PostDeleteListeners.Contains(postDeleteEventListener)) {
                    this.PostDeleteListeners.Add(postDeleteEventListener);
                }
            }
        }
    }
}