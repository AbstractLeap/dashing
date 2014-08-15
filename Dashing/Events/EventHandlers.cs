using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Events {
    public class EventHandlers {
        public EventHandlers(ICollection<IEventListener> listeners) {
            this.UpdateListeners(listeners);
            this.PreInsertListeners = new List<IOnPreInsertEventListener>();
            this.PreSaveListeners = new List<IOnPreSaveEventListener>();
            this.PreDeleteListeners = new List<IOnPreDeleteEventListener>();
            this.PostInsertListeners = new List<IOnPostInsertEventListener>();
            this.PostSaveListeners = new List<IOnPostSaveEventListener>();
            this.PostDeleteListeners = new List<IOnPostDeleteEventListener>();
        }

        public IList<IOnPreInsertEventListener> PreInsertListeners { get; private set; }

        public IList<IOnPreSaveEventListener> PreSaveListeners { get; private set; }

        public IList<IOnPreDeleteEventListener> PreDeleteListeners { get; private set; }

        public IList<IOnPostInsertEventListener> PostInsertListeners { get; private set; }

        public IList<IOnPostSaveEventListener> PostSaveListeners { get; private set; }

        public IList<IOnPostDeleteEventListener> PostDeleteListeners { get; private set; }

        internal void Invalidate(ICollection<IEventListener> listeners) {
            this.UpdateListeners(listeners);
        }

        private void UpdateListeners(ICollection<IEventListener> listeners) {
            foreach (var listener in listeners) {
                if (listener is IOnPreInsertEventListener) {
                    this.PreInsertListeners.Add(listener as IOnPreInsertEventListener);
                }

                if (listener is IOnPreSaveEventListener) {
                    this.PreSaveListeners.Add(listener as IOnPreSaveEventListener);
                }

                if (listener is IOnPreDeleteEventListener) {
                    this.PreDeleteListeners.Add(listener as IOnPreDeleteEventListener);
                }

                if (listener is IOnPostInsertEventListener) {
                    this.PostInsertListeners.Add(listener as IOnPostInsertEventListener);
                }

                if (listener is IOnPostSaveEventListener) {
                    this.PostSaveListeners.Add(listener as IOnPostSaveEventListener);
                }

                if (listener is IOnPostDeleteEventListener) {
                    this.PostDeleteListeners.Add(listener as IOnPostDeleteEventListener);
                }
            }
        }
    }
}
