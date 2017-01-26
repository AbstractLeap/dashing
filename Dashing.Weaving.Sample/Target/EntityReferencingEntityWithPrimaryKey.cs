namespace Dashing.Weaving.Sample.Target {
    using System.Collections.Generic;

    using Dashing.CodeGeneration;

    public class EntityReferencingEntityWithPrimaryKey : ISetLogger, ITrackedEntity {
        private bool isTracking;

        private int? __hashcode;

        protected bool __Name_IsSet;

        protected bool __Name_IsDirty;

        protected string __name;

        protected bool __isTracked;

        public long Id { get; set; }

        public string Name { get; set; }

        public string EntityWithStringPrimaryKeyId;

        private EntityWithStringPrimaryKey __entity;

        private EntityWithStringPrimaryKey __entity_OldValue;

        private bool __EntityWithStringPrimaryKey_IsDirty;

        public EntityWithStringPrimaryKey EntityWithStringPrimaryKey
        {
            get
            {
                if (this.__entity == null && this.EntityWithStringPrimaryKeyId != null) {
                    this.__entity = new EntityWithStringPrimaryKey { Id = this.EntityWithStringPrimaryKeyId };
                }

                return this.__entity;
            }

            set
            {
                this.EntityWithStringPrimaryKeyId = null;
                if (this.isTracking) {
                    if (!this.__EntityWithStringPrimaryKey_IsDirty) {
                        if ((this.__entity == null && value != null) || (this.__entity != null && !this.__entity.Equals(value))) {
                            this.__entity_OldValue = this.__entity;
                            this.__EntityWithStringPrimaryKey_IsDirty = true;
                        }
                    }
                }

                this.__entity = value;
            }
        }

        public IEnumerable<string> GetSetProperties() {
            throw new System.NotImplementedException();
        }

        public bool IsSetLoggingEnabled() {
            throw new System.NotImplementedException();
        }

        public void EnableSetLogging() {
            throw new System.NotImplementedException();
        }

        public void DisableSetLogging() {
            throw new System.NotImplementedException();
        }

        public void EnableTracking() {
            throw new System.NotImplementedException();
        }

        public void DisableTracking() {
            throw new System.NotImplementedException();
        }

        public bool IsTrackingEnabled() {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> GetDirtyProperties() {
            throw new System.NotImplementedException();
        }

        public object GetOldValue(string propertyName) {
            throw new System.NotImplementedException();
        }
    }
}