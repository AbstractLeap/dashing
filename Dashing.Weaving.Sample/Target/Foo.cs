namespace Dashing.Weaving.Sample.Target {
    using System;
    using System.Collections.Generic;
    using Dashing.CodeGeneration;

    public class Foo : ITrackedEntity {
        private bool isTracking = false;

        private bool __Name_IsDirty = false;

        private string __Name_OldValue = null;

        public int FooId { get; set; }

        public string Name { get; set; }
        
        public bool IsTrackingEnabled() {
            return this.isTracking;
        }

        public void EnableTracking() {
            this.isTracking = true;
        }

        public void DisableTracking() {
            this.isTracking = false;
            this.__Name_IsDirty = false;
        }

        public IEnumerable<string> GetDirtyProperties() {
            var dirtyProps = new List<string>();
            if (__Name_IsDirty) {
                dirtyProps.Add("Name");
            }

            return dirtyProps;
        }

        public object GetOldValue(string propertyName) {
            switch (propertyName) {
                case "Name":
                    if (this.__Name_IsDirty)
                        return this.__Name_OldValue;
                    break;
            }

            throw new ArgumentOutOfRangeException("propertyName", "Either the property doesn't exist or it's not dirty. Consult GetDirtyProperties first");
        }
    }
}