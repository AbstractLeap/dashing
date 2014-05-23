namespace TopHat.Extensions {
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions {
        public static bool IsEmpty<T>(this IEnumerable<T> source) {
            return !source.Any();
        }
    }
}