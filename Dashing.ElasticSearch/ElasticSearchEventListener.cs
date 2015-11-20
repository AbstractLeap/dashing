namespace Dashing.ElasticSearch {
    using System;
    using System.Collections.Generic;

    using Dashing.Events;

    using Nest;

    public class ElasticSearchEventListener : IOnPostInsertEventListener, IOnPostSaveEventListener, IOnPostDeleteEventListener {
        private readonly IElasticClientFactory factory;

        private readonly HashSet<Type> indexedTypes;

        public ElasticSearchEventListener(IElasticClientFactory factory) {
            this.factory = factory;
            this.indexedTypes = new HashSet<Type>();
        }

        public void AddTypeToIndex(Type type) {
            this.indexedTypes.Add(type);
        }

        public void OnPostDelete(object entity, ISession session) {
            this.MaybeDelete(entity, session);
        }

        public void OnPostSave(object entity, ISession session) {
            this.MaybeIndex(entity, session);
        }

        public void OnPostInsert(object entity, ISession session) {
            this.MaybeIndex(entity, session);
        }

        private void MaybeIndex(object entity, ISession session) {
            if (!this.indexedTypes.Contains(entity.GetType())) {
                return;
            }

            this.factory.Create().Index(entity, i => i.Type(TypeOf(entity)).Id(IdOf(entity, session)));
        }

        private void MaybeDelete(object entity, ISession session) {
            if (!this.indexedTypes.Contains(entity.GetType())) {
                return;
            }

            this.factory.Create().Delete(IdOf(entity, session), d => d.Type(TypeOf(entity)));
        }

        private static string IdOf(object entity, ISession session) {
            return session.Configuration.GetMap(entity.GetType()).GetPrimaryKeyValue(entity).ToString();
        }

        private static string TypeOf(object entity) {
            return entity.GetType().Name;
        }
    }
}