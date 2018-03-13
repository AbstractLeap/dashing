namespace Dashing.Migration {
    using System.Collections.Generic;

    using Dashing.Configuration;

    public class ColumnKeyValuePairEqualityComparer : IEqualityComparer<KeyValuePair<string, IColumn>> {
        public bool Equals(KeyValuePair<string, IColumn> x, KeyValuePair<string, IColumn> y) {
            return x.Key == y.Key;
        }

        public int GetHashCode(KeyValuePair<string, IColumn> obj) {
            return obj.Key.GetHashCode();
        }
    }
}