using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpecializedCollections {

    public class KeyedListDictionary<TKey, TValue> : IMultiValueDictionary<TKey, TValue> {
        private readonly Dictionary<TKey, ValueCollection> _items;

        public IReadOnlyCollection<TKey> Keys =>
            _items.Keys;

        public int Count =>
            _items.Count;

        IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] =>
            _items[key];

        public IGroupingCollection<TKey, TValue> this[TKey key] =>
            _items[key];

        public bool ContainsKey(TKey key) => _items.ContainsKey(key);

        public void Clear() => _items.Clear();

        public KeyedListDictionary(ILookup<TKey, TValue> items, IEqualityComparer<TKey> keyComparer) {
            _items = new Dictionary<TKey, ValueCollection>(keyComparer);
            foreach (var item in items) {
                _items.Add(item.Key, new ValueCollection(item.Key, item));
            }
        }

        public KeyedListDictionary(ILookup<TKey, TValue> items)
            : this(items, EqualityComparer<TKey>.Default) {

        }

        public KeyedListDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> keyComparer)
            : this(items.ToLookup(a => a.Key, a => a.Value, keyComparer)) {
        }

        public KeyedListDictionary(IEqualityComparer<TKey> keyComparer) {
            _items = new Dictionary<TKey, ValueCollection>(keyComparer);
        }

        public KeyedListDictionary()
            : this(EqualityComparer<TKey>.Default) {
        }

        private class ValueCollection : IGroupingCollection<TKey, TValue> {

            private readonly List<TValue> _values;

            public ValueCollection(TKey key, IEnumerable<TValue> values) {
                Key = key;
                _values = new List<TValue>(values);
            }

            public ValueCollection(TKey key)
                : this(key, Enumerable.Empty<TValue>()) {
            }

            public TKey Key { get; }

            public bool IsReadOnly => false;

            public int Count => _values.Count;

            public IEnumerator<TValue> GetEnumerator() {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _values.GetEnumerator();
            }

            public void Add(IEnumerable<TValue> items) {
                _values.AddRange(items);
            }

            public void Add(params TValue[] items) {
                Add((IEnumerable<TValue>)items);
            }

            public void Remove(IEnumerable<TValue> items) {                
                foreach (var item in items) {
                    _values.Remove(item);
                }
            }

            bool ICollection<TValue>.Remove(TValue item) {
                return _values.Remove(item);
            }

            void ICollection<TValue>.Add(TValue item) {
                _values.Add(item);
            }

            public void Clear() {
                _values.Clear();
            }

            public bool Contains(TValue item) {
                return _values.Contains(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex) {
                _values.CopyTo(array, arrayIndex);
            }
        }        

        private ValueCollection GetOrCreate(TKey key) {
            ValueCollection result;
            if (_items.TryGetValue(key, out result)) {
                return result;
            }

            result = new ValueCollection(key);
            _items.Add(key, result);
            return result;
        }

        public void Add(ILookup<TKey, TValue> items) {
            foreach (var item in items) {
                var values = GetOrCreate(item.Key);
                values.Add(item);
            }
        }

        bool ILookup<TKey, TValue>.Contains(TKey key) => ContainsKey(key);

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() {
            foreach (var key in Keys) {
                yield return this[key];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
