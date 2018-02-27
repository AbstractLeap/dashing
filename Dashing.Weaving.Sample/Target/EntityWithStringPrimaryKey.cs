namespace Dashing.Weaving.Sample.Target {
    using System;
    using System.Collections.Generic;

    using Dashing.CodeGeneration;

    public class EntityWithStringPrimaryKey : ISetLogger, ITrackedEntity {
        private int? __hashcode;

        protected bool __Name_IsSet;

        protected bool __isTracking;

        protected bool __Name_IsDirty;

        protected string __Name_OldValue;

        private string __name;

        public string Id { get; set; }

        public string Name
        {
            get
            {
                return this.__name;
            }

            set
            {
                if (this.__isTracking) {
                    if (!this.__Name_IsDirty && ((this.__name == null && value != null) || (this.__name != null && !this.__name.Equals(value)))) {
                        this.__Name_OldValue = this.__name;
                        this.__Name_IsDirty = true;
                    }
                }

                this.__Name_IsSet = true;
                this.__name = value;
            }
        }

        public override int GetHashCode() {
            int result;
            if (this.__hashcode.HasValue) {
                result = this.__hashcode.Value;
            }
            else {
                if (this.Id == null) {
                    this.__hashcode = base.GetHashCode();
                    result = this.__hashcode.Value;
                }
                else {
                    result = this.Id.GetHashCode();
                }
            }

            return result;
        }

        public override bool Equals(object obj) {
            bool result;
            if (obj == null) {
                result = false;
            }
            else {
                EntityWithStringPrimaryKey entityWithGuidPrimaryKey = obj as EntityWithStringPrimaryKey;
                result = (entityWithGuidPrimaryKey != null && this.Id != null && this.Id == entityWithGuidPrimaryKey.Id);
            }

            return result;
        }

        public static bool operator ==(EntityWithStringPrimaryKey left, EntityWithStringPrimaryKey right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if ((object)left == null || (object)right == null) {
                return false;
            }

            return left.Id == right.Id;
        }

        public static bool operator !=(EntityWithStringPrimaryKey left, EntityWithStringPrimaryKey right) {
            return !(left == right);
        }

        public IEnumerable<string> GetSetProperties() {
            List<string> list = new List<string>();
            if (this.__Name_IsSet) {
                list.Add("Name");
            }

            return list;
        }

        public bool IsSetLoggingEnabled() {
            throw new NotImplementedException();
        }

        public void EnableSetLogging() {
            throw new NotImplementedException();
        }

        public void DisableSetLogging() {
            throw new NotImplementedException();
        }

        public void EnableTracking() {
            this.__isTracking = true;
        }

        public void DisableTracking() {
            this.__isTracking = false;
            this.__Name_IsDirty = false;
            this.__Name_OldValue = null;
        }

        public bool IsTrackingEnabled() {
            return this.__isTracking;
        }

        public IEnumerable<string> GetDirtyProperties() {
            List<string> list = new List<string>();
            if (this.__Name_IsDirty) {
                list.Add("Name");
            }

            return list;
        }

        public object GetOldValue(string propertyName) {
            if (propertyName != null) {
                if (propertyName == "Name") {
                    if (this.__Name_IsDirty) {
                        return this.__Name_OldValue;
                    }
                }
            }

            throw new ArgumentOutOfRangeException("propertyName", "Either the property doesn't exist or it's not dirty. Consult GetDirtyProperties first");
        }
    }
}