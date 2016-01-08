namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Index {
        public Index(IMap map, ICollection<IColumn> columns, string name = null, bool isUnique = false) {
            if (map == null) {
                throw new ArgumentNullException("map");
            }

            if (columns == null) {
                throw new ArgumentNullException("columns");
            }

            this.Map = map;
            this.Columns = columns;
            this.IsUnique = isUnique;
            this.Name = !string.IsNullOrWhiteSpace(name) ? name : this.GenerateName();
        }

        private string GenerateName() {
            return "idx_" + this.Map.Type.Name + "_" + string.Join("_", this.Columns.Select(c => c.Name));
        }

        /// <summary>
        ///     The name of the index
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     The map that the index belongs to
        /// </summary>
        public IMap Map { get; private set; }

        /// <summary>
        ///     The list of columns that this index applies to
        /// </summary>
        /// <remarks>This is used in generating the hashcode so be careful when modifying</remarks>
        public ICollection<IColumn> Columns { get; private set; }

        /// <summary>
        ///     Indicates if the index is a unique one
        /// </summary>
        public bool IsUnique { get; private set; }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherIndex = obj as Index;
            if (otherIndex == null) {
                return false;
            }

            return this.Name == otherIndex.Name && this.IsUnique == otherIndex.IsUnique
                   && this.Columns.SequenceEqual(otherIndex.Columns, new IndexColumnComparer()) && this.Map.Type.Name == otherIndex.Map.Type.Name;
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + this.Name.GetHashCode();
                hash = hash * 23 + this.IsUnique.GetHashCode();
                foreach (var column in this.Columns) {
                    hash = hash * 23 + column.Name.GetHashCode();
                }

                hash = hash * 23 + this.Map.Type.Name.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Index a, Index b) {
            if (ReferenceEquals(a, b)) {
                return true;
            }

            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Name == b.Name && a.IsUnique == b.IsUnique && a.Columns.SequenceEqual(b.Columns, new IndexColumnComparer())
                   && a.Map.Type.Name == b.Map.Type.Name;
        }

        public static bool operator !=(Index a, Index b) {
            return !(a == b);
        }
    }
}