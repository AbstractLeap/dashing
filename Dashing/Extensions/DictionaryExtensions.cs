namespace Dashing.Extensions {
    using System;
    using System.Collections.Generic;

    public static class DictionaryExtensions {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueCreator) {
            // if you change the name of valueCreator I'd do a Find in Solution ;-)
            TValue value;
            if (!dictionary.TryGetValue(key, out value)) {
                value = valueCreator();
                dictionary.Add(key, value);
            }

            return value;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new() {
            return dictionary.GetOrAdd(key, () => new TValue());
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
            return dictionary.GetOrAdd(key, () => value);
        }
    }
}