using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Halliburton.IC.SpecializedCollections {
    public static class Bag {
        private static T Id<T>(T value) => value;

        public static void Add<TKey, TValue>(this IBag<TKey, TValue> dictionary, TKey key, IEnumerable<TValue> values) {
            dictionary.Add(values.ToLookup(_ => key, Id));
        }

        public static void Add<TKey, TValue>(this IBag<TKey, TValue> dictionary, TKey key, params TValue[] values) {
            dictionary.Add(values.ToLookup(_ => key, Id));
        }

        public static void Add<TKey, TValue>(this IBag<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items) {
            dictionary.Add(items.ToLookup(k => k.Key, k => k.Value));
        }
    }

    public class Bag<TKey, TValue> : IBag<TKey, TValue> {
        private readonly IDictionary<TKey, ValueCollection> _items;
        private readonly Func<IEnumerable<TValue>, ICollection<TValue>> _collectionConstructor;

        public ICollection<TKey> Keys =>
            _items.Keys;

        public int Count => _items.Count;

        IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => _items[key];

        public bool ContainsKey(TKey key) => _items.ContainsKey(key);

        public void Clear() => _items.Clear();

        public Bag(
          ILookup<TKey, TValue> items = null,
          IEqualityComparer<TKey> keyComparer = null,
          Func<IEnumerable<TValue>, ICollection<TValue>> collectionConstructor = null,
          Func<IEqualityComparer<TKey>, IDictionary<TKey, ValueCollection>> dictionaryConstructor = null) {
            _collectionConstructor = collectionConstructor ?? (a => new List<TValue>(a));
            dictionaryConstructor = dictionaryConstructor ?? (comparer => new Dictionary<TKey, ValueCollection>(comparer));
            _items = dictionaryConstructor(keyComparer ?? EqualityComparer<TKey>.Default);
            if (items != null) {
                foreach (var item in items) {
                    _items.Add(item.Key, new ValueCollection(item.Key, _collectionConstructor(item)));
                }
            }
        }

        public Bag(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> keyComparer = null, Func<IEnumerable<TValue>, ICollection<TValue>> collectionConstructor = null)
            : this(items.ToLookup(a => a.Key, a => a.Value), keyComparer, collectionConstructor) {
        }

        public class ValueCollection : ICollection<TValue>, IGrouping<TKey, TValue> {
            private readonly ICollection<TValue> _values = new HashSet<TValue>();

            internal ValueCollection(TKey key, ICollection<TValue> collection) {
                Key = key;
                _values = collection;
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
                foreach (var item in items) {
                    _values.Add(item);
                }
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

        public ValueCollection this[TKey key] {
            get {
                return GetOrCreate(key);
            }
        }

        private ValueCollection GetOrCreate(TKey key) {
            if (_items.TryGetValue(key, out var result)) {
                return result;
            }

            result = new ValueCollection(key, _collectionConstructor(Enumerable.Empty<TValue>()));
            _items.Add(key, result);
            return result;
        }

        public void Add(ILookup<TKey, TValue> items) {
            foreach (var item in items) {
                var values = GetOrCreate(item.Key);
                values.Add(item);
            }
        }

        private class Grouping : IGrouping<TKey, TValue> {
            private readonly IEnumerable<TValue> _values;
            public TKey Key { get; }

            public Grouping(TKey key, ValueCollection values) {
                _values = values;
                Key = key;
            }

            public IEnumerator<TValue> GetEnumerator() {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return _values.GetEnumerator();
            }
        }

        bool ILookup<TKey, TValue>.Contains(TKey key) => ContainsKey(key);

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() {
            foreach (var item in _items) {
                yield return new Grouping(item.Key, item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public ILookup<TKey, TValue> ToLookup() {
            var items = from item in _items
                        from Value in item.Value
                        select new {
                            item.Key,
                            Value
                        };

            return items.ToLookup(a => a.Key, a => a.Value);
        }
    }
}
