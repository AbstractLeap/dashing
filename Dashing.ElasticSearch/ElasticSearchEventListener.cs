using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashing.Events;
using Nest;

namespace Dashing.ElasticSearch {
    public class ElasticSearchEventListener : IOnPostInsertEventListener, IOnPostSaveEventListener, IOnPostDeleteEventListener {
        private IClientFactory factory;

        private HashSet<Type> types;

        public ElasticSearchEventListener(IClientFactory factory) {
            this.factory = factory;
            this.types = new HashSet<Type>();
        }

        public void AddTypeToIndex(Type type) {
            this.types.Add(type);
        }

        public void AddTypesToIndex(IEnumerable<Type> types) {
            foreach (var type in types) {
                this.AddTypeToIndex(type);
            }
        }

        public void OnPostDelete(object entity, ISession session) {
            if (this.types.Contains(entity.GetType())) {
                this.factory.Create().DeleteAsync(session.Configuration.GetMap(entity.GetType()).GetPrimaryKeyValue(entity).ToString(), d => d.Type(entity.GetType().Name)).Wait();
            }
        }

        public void OnPostSave(object entity, ISession session) {
            if (this.types.Contains(entity.GetType())) {
                this.factory.Create().IndexAsync(entity, i => i.Type(entity.GetType().Name).Id(session.Configuration.GetMap(entity.GetType()).GetPrimaryKeyValue(entity).ToString())).Wait();
            }
        }

        public void OnPostInsert(object entity, ISession session) {
            if (this.types.Contains(entity.GetType())) {
                this.factory.Create().IndexAsync(entity, i => i.Type(entity.GetType().Name).Id(session.Configuration.GetMap(entity.GetType()).GetPrimaryKeyValue(entity).ToString())).Wait();
            }
        }
    }
}
