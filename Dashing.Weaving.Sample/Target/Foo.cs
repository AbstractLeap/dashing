namespace Dashing.Weaving.Sample.Target {
    using System;
    using System.Collections.Generic;

    using Dashing.CodeGeneration;
    using Dashing.Weaving.Sample.Domain;

    public class Foo : ITrackedEntity {
        private bool isTracking;

        private bool __Name_IsDirty;

        private string __Name_OldValue;

        private bool __IsBar_IsDirty;

        private bool? __IsBar_OldValue;

        private bool __IsRah_IsDirty;

        private bool? __IsRah_OldValue;

        private bool __Type_IsDirty;

        private FooType? __Type_OldValue;

        public int FooId { get; set; }

        private string __name;

        private bool __isBar;

        private bool? __IsRah;

        private FooType __type;

        public string Name
        {
            get
            {
                return this.__name;
            }

            set
            {
                if (this.isTracking) {
                    if (!this.__Name_IsDirty) {
                        if ((this.__name == null && value != null) || (this.__name != null && !this.__name.Equals(value))) {
                            this.__Name_OldValue = this.__name;
                            this.__Name_IsDirty = true;
                        }
                    }
                }

                this.__name = value;
            }
        }

        public bool IsBar
        {
            get
            {
                return this.__isBar;
            }
            set
            {
                if (this.isTracking) {
                    if (!this.__IsBar_IsDirty) {
                        if (!this.__isBar.Equals(value)) {
                            this.__IsBar_OldValue = this.__isBar;
                            this.__IsBar_IsDirty = true;
                        }
                    }
                }

                this.__isBar = value;
            }
        }

        public bool? IsRah
        {
            get
            {
                return this.__IsRah;
            }

            set
            {
                if (this.isTracking) {
                    if (!this.__IsRah_IsDirty) {
                        if (!this.__IsRah.Equals(value)) {
                            this.__IsRah_OldValue = this.__IsRah;
                            this.__IsRah_IsDirty = true;
                        }
                    }
                }

                this.__IsRah = value;
            }
        }

        public FooType Type
        {
            get
            {
                return this.__type;
            }

            set
            {
                if (this.isTracking) {
                    if (!this.__Type_IsDirty) {
                        if (!this.__type.Equals(value)) {
                            this.__Type_OldValue = this.__type;
                            this.__Type_IsDirty = true;
                        }
                    }
                }

                this.__type = value;
            }
        }

        public bool IsTrackingEnabled() {
            return this.isTracking;
        }

        public void EnableTracking() {
            this.isTracking = true;
        }

        public void DisableTracking() {
            this.isTracking = false;
            this.__Name_IsDirty = false;
            this.__Name_OldValue = null;
            this.__IsBar_IsDirty = false;
            this.__IsBar_OldValue = null;
            this.__IsRah_IsDirty = false;
            this.__IsRah_OldValue = null;
        }

        public IEnumerable<string> GetDirtyProperties() {
            var dirtyProps = new List<string>();
            if (__Name_IsDirty) {
                dirtyProps.Add("Name");
            }

            if (__IsBar_IsDirty) {
                dirtyProps.Add("IsBar");
            }

            if (__IsRah_IsDirty) {
                dirtyProps.Add("IsRah");
            }

            return dirtyProps;
        }

        public object GetOldValue(string propertyName) {
            switch (propertyName) {
                case "Name":
                    if (this.__Name_IsDirty) {
                        return this.__Name_OldValue;
                    }

                    break;

                case "IsBar":
                    if (this.__IsBar_IsDirty) {
                        return this.__IsBar_OldValue;
                    }

                    break;

                case "IsRah":
                    if (this.__IsRah_IsDirty) {
                        return this.__IsRah_OldValue;
                    }

                    break;
            }

            throw new ArgumentOutOfRangeException(
                "propertyName",
                "Either the property doesn't exist or it's not dirty. Consult GetDirtyProperties first");
        }
    }
}