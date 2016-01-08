namespace Dashing.Configuration {
    using System;

    public class ForeignKey {
        public ForeignKey(IMap parentMap, IColumn childColumn, string name = null) {
            if (parentMap == null) {
                throw new ArgumentNullException("parentMap");
            }

            if (childColumn == null) {
                throw new ArgumentNullException("childColumn");
            }

            this.ParentMap = parentMap;
            this.ChildColumn = childColumn;
            this.Name = !string.IsNullOrWhiteSpace(name) ? name : this.GenerateName();
        }

        private string GenerateName() {
            return "fk_" + this.ChildColumn.Map.Type.Name + "_" + this.ParentMap.Type.Name + "_" + this.ChildColumn.Name;
        }

        public IMap ParentMap { get; private set; }

        public IColumn ChildColumn { get; private set; }

        public string Name { get; private set; }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherForeignKey = obj as ForeignKey;
            if (otherForeignKey == null) {
                return false;
            }

            return this.Name == otherForeignKey.Name && this.ChildColumn.Name == otherForeignKey.ChildColumn.Name
                   && this.ChildColumn.Map.Type.Name == otherForeignKey.ChildColumn.Map.Type.Name
                   && this.ParentMap.Type.Name == otherForeignKey.ParentMap.Type.Name;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + this.Name.GetHashCode();
                hash = hash * 23 + this.ChildColumn.Name.GetHashCode();
                hash = hash * 23 + this.ChildColumn.Map.Type.Name.GetHashCode();
                hash = hash * 23 + this.ParentMap.Type.Name.GetHashCode();
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

            return a.Name == b.Name && a.ChildColumn.Name == b.ChildColumn.Name && a.ChildColumn.Map.Type.Name == b.ChildColumn.Map.Type.Name
                   && a.ParentMap.Type.Name == b.ParentMap.Type.Name;
        }

        public static bool operator !=(ForeignKey a, ForeignKey b) {
            return !(a == b);
        }
    }
}