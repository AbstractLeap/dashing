namespace Dashing.Configuration {
    using System.Collections.Generic;

    internal class IndexColumnComparer : IEqualityComparer<IColumn> {
        public bool Equals(IColumn x, IColumn y) {
            return x.Name == y.Name;
        }

        public int GetHashCode(IColumn obj) {
            return obj.Name.GetHashCode();
        }
    }
}