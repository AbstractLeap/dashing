namespace Dashing.Weaving.Sample.Target {
    using System;
    using System.Collections.Generic;

    using Dashing.CodeGeneration;

    public class Bar : ITrackedEntity {
        private bool isTracking;

        private bool __Name_IsDirty;

        private string __Name_OldValue;

        private bool __Foo_IsDirty;

        private Foo __Foo_OldValue;

        private string __name;

        private Foo __foo;

        public int Id { get; set; }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherBar = obj as Bar;
            if (otherBar == null) {
                return false;
            }

            if (this.Id == default(int)) {
                return false;
            }

            return this.Id == otherBar.Id;
        }

        private int? __hashCode;

        public override int GetHashCode() {
            if (this.__hashCode.HasValue) {
                return this.__hashCode.Value;
            }

            if (this.Id == default(int)) {
                this.__hashCode = base.GetHashCode();
                return this.__hashCode.Value;
            }
            else {
                unchecked {
                    return 17 * 29 + this.Id;
                }
            }
        }

        public string Name {
            get {
                return this.__name;
            }
            set {
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

        public int? FooId;

        public Foo Foo {
            get {
                if (this.__foo == null && this.FooId.HasValue) {
                    this.__foo = new Foo { FooId = this.FooId.Value };
                }

                return this.__foo;
            }
            set {
                this.FooId = null;
                if (this.isTracking) {
                    if (!this.__Foo_IsDirty) {
                        if ((this.__foo == null && value != null) || (this.__foo != null && !this.__foo.Equals(value))) {
                            this.__Foo_OldValue = this.__foo;
                            this.__Foo_IsDirty = true;
                        }
                    }
                }

                this.__foo = value;
            }
        }

        public void EnableTracking() {
            this.isTracking = true;
        }

        public void DisableTracking() {
            this.isTracking = false;
            this.__Foo_IsDirty = false;
            this.__Foo_OldValue = null;
            this.__Name_IsDirty = false;
            this.__Name_OldValue = null;
        }

        public bool IsTrackingEnabled() {
            return this.isTracking;
        }

        public IEnumerable<string> GetDirtyProperties() {
            var dirtyProps = new List<string>();
            if (__Name_IsDirty) {
                dirtyProps.Add("Name");
            }

            if (__Foo_IsDirty) {
                dirtyProps.Add("Foo");
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

                case "IsRah":
                    if (this.__Foo_IsDirty) {
                        return this.__Foo_OldValue;
                    }

                    break;
            }

            throw new ArgumentOutOfRangeException("propertyName", "Either the property doesn't exist or it's not dirty. Consult GetDirtyProperties first");
        }
    }
}