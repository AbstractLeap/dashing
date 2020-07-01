namespace Dashing.Extensions {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     A generic object comparerer that would only use object's reference, taken from Castle Core.
    /// </summary>
    public class ReferenceEqualityComparer<T> : IEqualityComparer, IEqualityComparer<T> {
        private static readonly ReferenceEqualityComparer<T> instance = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer() { }

        public int GetHashCode(object obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }

        bool IEqualityComparer.Equals(object x, object y) {
            return x == y;
        }

        bool IEqualityComparer<T>.Equals(T x, T y) {
            return (object)x == (object)y;
        }

        int IEqualityComparer<T>.GetHashCode(T obj) {
            return RuntimeHelpers.GetHashCode(obj);
        }

        public static ReferenceEqualityComparer<T> Instance => instance;
    }
}