namespace Dashing.Migration {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;

    public class TableNameEqualityComparer : IEqualityComparer<IMap> {
        public bool Equals(IMap x, IMap y) {
            return x.Table.Equals(y.Table, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IMap obj) {
            return obj.Table.ToLowerInvariant().GetHashCode();
        }
    }
}