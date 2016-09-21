namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Engine.Dialects;

    public class Index {
        private readonly string name;

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
            this.name = !string.IsNullOrWhiteSpace(name) ? name : null;
        }

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
        
        internal string Name {
            get {
                return this.name;
            }
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            var otherIndex = obj as Index;
            if (otherIndex == null) {
                return false;
            }

            return this.IsUnique == otherIndex.IsUnique
                   && this.Columns.SequenceEqual(otherIndex.Columns, new IndexColumnComparer()) && this.Map.Type.Name.Equals(otherIndex.Map.Type.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash = hash * 23 + this.IsUnique.GetHashCode();
                foreach (var column in this.Columns) {
                    hash = hash * 23 + column.Name.ToLowerInvariant().GetHashCode();
                }

                hash = hash * 23 + this.Map.Type.Name.ToLowerInvariant().GetHashCode();
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

            return a.Equals(b);
        }

        public static bool operator !=(Index a, Index b) {
            return !(a == b);
        }
    }
}