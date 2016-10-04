namespace Dashing.Extensions {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;

    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
                                                   IList<KeyValuePair<TKey, TValue>>,
                                                   ICollection<KeyValuePair<TKey, TValue>>,
                                                   IEnumerable<KeyValuePair<TKey, TValue>>,
                                                   IEnumerable {
        private readonly Dictionary<TKey, TValue> _dictionary;

        private readonly List<TKey> _keys;

        private readonly List<TValue> _values;

        public int Count
        {
            get
            {
                return this._dictionary.Count;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return this._keys.AsReadOnly();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                this.RemoveFromLists(key);
                this._dictionary[key] = value;
                this._keys.Add(key);
                this._values.Add(value);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return this._values.AsReadOnly();
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).IsReadOnly;
            }
        }

        public OrderedDictionary()
            : this(0) {
        }

        public OrderedDictionary(int capacity) {
            this._dictionary = new Dictionary<TKey, TValue>(capacity);
            this._keys = new List<TKey>(capacity);
            this._values = new List<TValue>(capacity);
        }

        public void Add(TKey key, TValue value) {
            this._dictionary.Add(key, value);
            this._keys.Add(key);
            this._values.Add(value);
        }

        public void Clear() {
            this._dictionary.Clear();
            this._keys.Clear();
            this._values.Clear();
        }

        public bool ContainsKey(TKey key) {
            return this._dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value) {
            return this._dictionary.ContainsValue(value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            int num = 0;
            foreach (var current in this._keys) {
                yield return new KeyValuePair<TKey, TValue>(current, this._values[num]);
                num++;
            }
        }

        private void RemoveFromLists(TKey key) {
            int num = this._keys.IndexOf(key);
            if (num != -1) {
                this._keys.RemoveAt(num);
                this._values.RemoveAt(num);
            }
        }

        public bool Remove(TKey key) {
            this.RemoveFromLists(key);
            return this._dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return this._dictionary.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            this.Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            bool flag = ((ICollection<KeyValuePair<TKey, TValue>>)this._dictionary).Remove(item);
            if (flag) {
                this.RemoveFromLists(item.Key);
            }
            return flag;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public int IndexOf(KeyValuePair<TKey, TValue> item) {
            var keyIndex = this._keys.IndexOf(item.Key);
            if (keyIndex > -1) {
                if (this._values[keyIndex].Equals(item.Value)) {
                    return keyIndex;
                }
            }

            return -1;
        }

        public void Insert(int index, KeyValuePair<TKey, TValue> item) {
            this._keys.Insert(index, item.Key);
            this._values.Insert(index, item.Value);
            this._dictionary.Add(item.Key, item.Value);
        }

        public void RemoveAt(int index) {
            var key = this._keys[index];
            this._dictionary.Remove(key);
            this._keys.RemoveAt(index);
            this._values.RemoveAt(index);
        }

        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get
            {
                if (index >= this.Count || index < 0) {
                    throw new ArgumentOutOfRangeException("index");
                }

                return new KeyValuePair<TKey, TValue>(this._keys[index], this._values[index]);
            }
            set
            {
                if (index >= this.Count || index < 0) {
                    throw new ArgumentOutOfRangeException("index");
                }

                this._keys[index] = value.Key;
                this._values[index] = value.Value;
                this._dictionary[value.Key] = value.Value;
            }
        }
    }
}