using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Threading {
    public class KeyedProducerConsumer<TKey, TItem> {
        private readonly Dictionary<TKey, TItem> _items;

        private readonly Dictionary<TKey, List<ConsumerReadAwaiter>> _notifications =
            new Dictionary<TKey, List<ConsumerReadAwaiter>>();

        private readonly ReaderWriterLock _lock =
            new ReaderWriterLock();

        public class ConsumerReadAwaiter : INotifyCompletion {
            private readonly object _lock = new object();
            private TItem _result;
            private long _isCompleted;
            private Action _continuations;

            public ConsumerReadAwaiter GetAwaiter() {
                return this;
            }

            public void OnCompleted(Action continuation) {
                _continuations += continuation;
            }

            public bool IsCompleted {
                get {
                    return _isCompleted != 0;
                }
            }

            public TItem GetResult() {
                return _result;
            }

            public void SetResult(TItem item) {
                if (Interlocked.CompareExchange(ref _isCompleted, 1, 0) == 0) {
                    _result = item;
                    _continuations?.Invoke();
                }
            }
        }

        public KeyedProducerConsumer(IEnumerable<KeyValuePair<TKey, TItem>> items, IEqualityComparer<TKey> comparer) {
            _items = new Dictionary<TKey, TItem>(comparer);
            foreach (var item in items) {
                _items.Add(item.Key, item.Value);
            }
        }

        public KeyedProducerConsumer(IEnumerable<KeyValuePair<TKey, TItem>> items)
            : this(items, EqualityComparer<TKey>.Default) {

        }

        public KeyedProducerConsumer(IEqualityComparer<TKey> comparer)
            : this(Enumerable.Empty<KeyValuePair<TKey, TItem>>()) {

        }

        public KeyedProducerConsumer()
            : this(Enumerable.Empty<KeyValuePair<TKey, TItem>>()) {

        }

        public void AddOrReplace(TKey key, TItem item) {
            try {
                _lock.AcquireWriterLock(Timeout.Infinite);
                _items.Add(key, item);
                lock (_notifications) {
                    List<ConsumerReadAwaiter> awaiters;
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

        public ConsumerReadAwaiter GetItem(TKey key) {
            try {
                _lock.AcquireReaderLock(Timeout.Infinite);
                TItem item;
                if (_items.TryGetValue(key, out item)) {
                    var awaiter = new ConsumerReadAwaiter();
                    awaiter.SetResult(item);
                    return awaiter;
                }
                else {
                    lock (_notifications) {
                        List<ConsumerReadAwaiter> awaiters;
                        if (!_notifications.TryGetValue(key, out awaiters)) {
                            awaiters = new List<ConsumerReadAwaiter>();
                            _notifications.Add(key, awaiters);
                        }
                        var awaiter = new ConsumerReadAwaiter();
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
