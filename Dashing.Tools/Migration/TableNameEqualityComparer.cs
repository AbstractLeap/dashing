namespace Dashing.Tools.Migration {
    using System.Collections.Generic;

    using Dashing.Configuration;

    public class TableNameEqualityComparer : IEqualityComparer<IMap> {
        public bool Equals(IMap x, IMap y) {
            return x.Table.Equals(y.Table);
        }

        public int GetHashCode(IMap obj) {
            return obj.Table.GetHashCode();
        }
    }
}