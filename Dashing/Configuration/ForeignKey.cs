namespace Dashing.Configuration {
    using System;

    public class ForeignKey {
        private readonly string name;

        public ForeignKey(IMap parentMap, IColumn childColumn, string name = null) {
            if (parentMap == null) {
                throw new ArgumentNullException("parentMap");
            }

            if (childColumn == null) {
                throw new ArgumentNullException("childColumn");
            }

            this.ParentMap = parentMap;
            this.ChildColumn = childColumn;
            this.name = !string.IsNullOrWhiteSpace(name) ? name : null;
        }

        public IMap ParentMap { get; private set; }

        public IColumn ChildColumn { get; private set; }

        internal string Name {
            get {
                return this.name;
            }
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherForeignKey = obj as ForeignKey;
            if (otherForeignKey == null) {
                return false;
            }

            return this.ChildColumn.Name.Equals(otherForeignKey.ChildColumn.Name, StringComparison.InvariantCultureIgnoreCase)
                   && this.ChildColumn.Map.Type.Name.Equals(otherForeignKey.ChildColumn.Map.Type.Name, StringComparison.InvariantCultureIgnoreCase)
                   && this.ParentMap.Type.Name.Equals(otherForeignKey.ParentMap.Type.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + this.ChildColumn.Name.ToLowerInvariant().GetHashCode();
                hash = hash * 23 + this.ChildColumn.Map.Type.Name.ToLowerInvariant().GetHashCode();
                hash = hash * 23 + this.ParentMap.Type.Name.ToLowerInvariant().GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ForeignKey a, ForeignKey b) {
            if (ReferenceEquals(a, b)) {
                return true;
            }

            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(ForeignKey a, ForeignKey b) {
            return !(a == b);
        }
    }
}