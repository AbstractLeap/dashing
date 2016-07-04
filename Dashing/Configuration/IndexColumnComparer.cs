namespace Dashing.Configuration {
    using System;
    using System.Collections.Generic;

    internal class IndexColumnComparer : IEqualityComparer<IColumn> {
        public bool Equals(IColumn x, IColumn y) {
            return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(IColumn obj) {
            return obj.Name.GetHashCode();
        }
    }
}