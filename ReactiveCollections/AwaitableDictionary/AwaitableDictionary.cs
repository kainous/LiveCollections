using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AwaitableDictionary {
    public class AwaitableDictionary<TKey, TItem> {
        private readonly Dictionary<TKey, TItem> _items;

        private readonly Dictionary<TKey, List<DictionaryReadAwaiter>> _notifications =
            new Dictionary<TKey, List<DictionaryReadAwaiter>>();

        private readonly ReaderWriterLock _lock =
            new ReaderWriterLock();

        public class DictionaryReadAwaiter : INotifyCompletion {
            private readonly object _lock = new object();
            private TItem _result;
            private Action _continuations;

            public DictionaryReadAwaiter GetAwaiter() {
                return this;
            }

            public void OnCompleted(Action continuation) {
                _continuations += continuation;
            }

            public bool IsCompleted { get; private set; }

            public TItem GetResult() {
                return _result;
            }

            public void SetResult(TItem item) {
                _result = item;
                IsCompleted = true;
                _continuations?.Invoke();
            }
        }

        public AwaitableDictionary(IEnumerable<KeyValuePair<TKey, TItem>> items, IEqualityComparer<TKey> comparer) {
            _items = new Dictionary<TKey, TItem>(comparer);
            foreach (var item in items) {
                _items.Add(item.Key, item.Value);
            }
        }

        public AwaitableDictionary(IEnumerable<KeyValuePair<TKey, TItem>> items)
            : this(items, EqualityComparer<TKey>.Default) {

        }

        public AwaitableDictionary(IEqualityComparer<TKey> comparer)
            : this(Enumerable.Empty<KeyValuePair<TKey, TItem>>()) {

        }

        public AwaitableDictionary()
            : this(Enumerable.Empty<KeyValuePair<TKey, TItem>>()) {

        }

        public void AddOrReplace(TKey key, TItem item) {
            try {
                _lock.AcquireWriterLock(Timeout.Infinite);
                _items.Add(key, item);
                lock (_notifications) {
                    List<DictionaryReadAwaiter> awaiters;
                    if (_notifications.TryGetValue(key, out awaiters)) {
                        foreach (var awaiter in awaiters) {
                            awaiter.SetResult(item);
                        }
                        _notifications.Remove(key);
                    }
                }
            }
            finally {
                _lock.ReleaseWriterLock();
            }
        }

        public DictionaryReadAwaiter GetItem(TKey key) {
            try {
                _lock.AcquireReaderLock(Timeout.Infinite);
                TItem item;
                if (_items.TryGetValue(key, out item)) {
                    var awaiter = new DictionaryReadAwaiter();
                    awaiter.SetResult(item);
                    return awaiter;
                }
                else {
                    lock (_notifications) {
                        List<DictionaryReadAwaiter> awaiters;
                        if (!_notifications.TryGetValue(key, out awaiters)) {
                            awaiters = new List<DictionaryReadAwaiter>();
                            _notifications.Add(key, awaiters);
                        }
                        var awaiter = new DictionaryReadAwaiter();
                        awaiters.Add(awaiter);
                        return awaiter;
                    }
                }
            }
            finally {
                _lock.ReleaseReaderLock();
            }
        }
    }
}
