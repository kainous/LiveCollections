using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Monadic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace SpecializedCollections {
    public interface IMultiKeyDictionary<TUpperKey, TLowerKey, TValue> {
        Task AddOrUpdateMany(ILookup<TUpperKey, TLowerKey> keys, Func<TUpperKey, TLowerKey, TValue> addValueFactory, Func<TUpperKey, TLowerKey, TValue, TValue> updateValueFactory, CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<TLowerKey, TValue>> GetGrouping(TUpperKey upperKey, CancellationToken cancellationToken = default);
        IObservable<(TUpperKey, TLowerKey, TValue)> GetItems(CancellationToken cancellationToken = default);
        IObservable<(TUpperKey, TLowerKey, Option<TValue>)> GetItems(ILookup<TUpperKey, TLowerKey> keys, CancellationToken cancellationToken = default);
        IObservable<(TUpperKey, TLowerKey, Option<TValue>)> TryRemove(ILookup<TUpperKey, TLowerKey> keys = null, Func<TUpperKey, TLowerKey, Option<TValue>, bool> predicate = null, CancellationToken cancellationToken = default);
        Task Clear(CancellationToken token);
    }

    public class MultiKeyDictionary<TUpperKey, TLowerKey, TValue> : IMultiKeyDictionary<TUpperKey, TLowerKey, TValue> {
        private readonly IDictionary<TUpperKey, IDictionary<TLowerKey, TValue>> _items =
            new Dictionary<TUpperKey, IDictionary<TLowerKey, TValue>>();        

        public virtual Task AddOrUpdateMany(ILookup<TUpperKey, TLowerKey> keys, Func<TUpperKey, TLowerKey, TValue> addValueFactory, Func<TUpperKey, TLowerKey, TValue, TValue> updateValueFactory, CancellationToken cancellationToken = default) {
            foreach (var upper in keys) {
                if (!_items.TryGetValue(upper.Key, out var lower)) {
                    lower = new Dictionary<TLowerKey, TValue>();
                    _items.Add(upper.Key, lower);
                }

                foreach (var key in upper) {
                    TValue newValue;
                    if (cancellationToken.IsCancellationRequested) {
                        return Task.CompletedTask;
                    }

                    if (lower.TryGetValue(key, out var previousValue)) {
                        newValue = updateValueFactory(upper.Key, key, previousValue);
                    }
                    else {
                        newValue = addValueFactory(upper.Key, key);
                    }

                    // Cancellation is used here in order to cancel TryAdd or TryUpdate
                    if (cancellationToken.IsCancellationRequested) {
                        return Task.CompletedTask;
                    }
                    lower[key] = newValue;
                }
            }

            return Task.CompletedTask;
        }

        public virtual Task<IReadOnlyDictionary<TLowerKey, TValue>> GetGrouping(TUpperKey upperKey, CancellationToken cancellationToken = default) =>
            cancellationToken.IsCancellationRequested
          ? Task.FromCanceled<IReadOnlyDictionary<TLowerKey, TValue>>(cancellationToken)
          : Task.FromResult((IReadOnlyDictionary<TLowerKey, TValue>)_items[upperKey]);

        public virtual IObservable<(TUpperKey, TLowerKey, Option<TValue>)> GetItems(ILookup<TUpperKey, TLowerKey> keys, CancellationToken cancellationToken = default) {
            return Observable.Create<(TUpperKey, TLowerKey, Option<TValue>)>(obs => {
                var items = from upperKey in keys
                            from lowerKey in upperKey
                            let upper = _items.TryGetValue(upperKey.Key)
                            from lower in upper
                            let item = lower.TryGetValue(lowerKey)
                            select (upperKey.Key, lowerKey, item);

                foreach (var item in items) {
                    if (cancellationToken.IsCancellationRequested) {
                        return null;
                    }

                    obs.OnNext(item);
                }

                return (IDisposable)null;
            });
        }

        public virtual Task Clear(CancellationToken cancellationToken = default) {
            if (!cancellationToken.IsCancellationRequested) {
                _items.Clear();
            }
            return Task.CompletedTask;
        }

        public virtual IObservable<(TUpperKey, TLowerKey, TValue)> GetItems(CancellationToken cancellationToken = default) {
            return Observable.Create<(TUpperKey, TLowerKey, TValue)>(obs => {
                var items = from upper in _items
                            from lower in upper.Value
                            select (upper.Key, lower.Key, lower.Value);

                foreach (var item in items) {
                    if (cancellationToken.IsCancellationRequested) {
                        return null;
                    }

                    obs.OnNext(item);
                }

                return (IDisposable)null;
            });
        }

        public virtual IObservable<(TUpperKey, TLowerKey, Option<TValue>)> TryRemove(ILookup<TUpperKey, TLowerKey> keys = null, Func<TUpperKey, TLowerKey, Option<TValue>, bool> predicate = null, CancellationToken cancellationToken = default) {
            return Observable.Create<(TUpperKey, TLowerKey, Option<TValue>)>(obs => {
                keys = keys ?? (_items.SelectMany(a => a.Value.Select(b => (a.Key, b.Key))).ToLookup(a => a.Item1, a => a.Item2));
                predicate = predicate ?? ((_, __, ___) => true);

                foreach (var upperKey in keys) {
                    if (_items.TryGetValue(upperKey.Key, out var upper)) {
                        foreach (var lowerKey in upperKey) {
                            if (cancellationToken.IsCancellationRequested) {
                                return null;
                            }
                            else if (upper.TryGetValue(lowerKey, out var lower)) {
                                obs.OnNext((upperKey.Key, lowerKey, lower.AsOption()));
                            }
                            else {
                                obs.OnNext((upperKey.Key, lowerKey, Option.None<TValue>()));
                            }
                        }
                    }
                    else {
                        foreach (var lowerKey in upperKey) {
                            if (cancellationToken.IsCancellationRequested) {
                                return null;
                            }
                            obs.OnNext((upperKey.Key, lowerKey, Option.None<TValue>()));
                        }
                    }
                }

                return (IDisposable)null;
            });
        }
    }

    public class ReadWriteLockedMultiKeyDictionary<TUpperKey, TLowerKey, TValue> : MultiKeyDictionary<TUpperKey, TLowerKey, TValue> {
        private readonly AsyncReaderWriterLock _lock
            = new AsyncReaderWriterLock();

        public override async Task AddOrUpdateMany(ILookup<TUpperKey, TLowerKey> keys, Func<TUpperKey, TLowerKey, TValue> addValueFactory, Func<TUpperKey, TLowerKey, TValue, TValue> updateValueFactory, CancellationToken cancellationToken = default) {
            using (await _lock.WriterLockAsync(cancellationToken)) {
                await base.AddOrUpdateMany(keys, addValueFactory, updateValueFactory);
            }
        }

        public override async Task<IReadOnlyDictionary<TLowerKey, TValue>> GetGrouping(TUpperKey upperKey, CancellationToken cancellationToken = default) {
            using (await _lock.ReaderLockAsync(cancellationToken)) {
                var result = await base.GetGrouping(upperKey, cancellationToken);
                // A snapshot copy is made here, because we cannot permit the user to access what is under the lock
                return new Dictionary<TLowerKey, TValue>(result.ToDictionary(a => a.Key, a => a.Value));
            }
        }

        public override IObservable<(TUpperKey, TLowerKey, Option<TValue>)> GetItems(ILookup<TUpperKey, TLowerKey> keys, CancellationToken cancellationToken = default) {
            return Observable.Using(
                resourceFactoryAsync: async token => {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                    return await _lock.ReaderLockAsync(cts.Token);
                },
                observableFactoryAsync: (disp, token) => {
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                    return Task.FromResult(base.GetItems(keys, cts.Token));
                });
        }

        public override IObservable<(TUpperKey, TLowerKey, TValue)> GetItems(CancellationToken cancellationToken = default) {
            return Observable.Using(
               resourceFactoryAsync: async token => {
                   var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                   return await _lock.ReaderLockAsync(cts.Token);
               },
               observableFactoryAsync: (disp, token) => {
                   var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                   return Task.FromResult(base.GetItems(cts.Token));
               });
        }

        public override async Task Clear(CancellationToken cancellationToken = default) {
            using (await _lock.WriterLockAsync(cancellationToken)) {
                await base.Clear(cancellationToken);
            }
        }

        public override IObservable<(TUpperKey, TLowerKey, Option<TValue>)> TryRemove(ILookup<TUpperKey, TLowerKey> keys = null, Func<TUpperKey, TLowerKey, Option<TValue>, bool> predicate = null, CancellationToken cancellationToken = default) {
            return Observable.Using(
               resourceFactoryAsync: async token => {
                   var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                   return await _lock.WriterLockAsync(cts.Token);
               },
               observableFactoryAsync: (disp, token) => {
                   var cts = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                   return Task.FromResult(base.TryRemove(keys, predicate, cancellationToken));
               });
        }
    }

    public static class MultiKeyDictionary {
        public static async Task AddOrUpdateMany<TUpperKey, TLowerKey, TValue>(this MultiKeyDictionary<TUpperKey, TLowerKey, TValue> dictionary, IEnumerable<(TUpperKey, TLowerKey, TValue)> items, CancellationToken cancellationToken = default) {
            // Prestructure the items for handling results under a potential lock
            var itemLookup = items.ToDictionary(a => (a.Item1, a.Item2), a => a.Item3);
            var keyLookup = itemLookup.Keys.ToLookup(a => a.Item1, a => a.Item2);
            await dictionary.AddOrUpdateMany(keyLookup, (a, b) => itemLookup[(a, b)], (a, b, _) => itemLookup[(a, b)], cancellationToken);
        }

        /// <summary>
        /// Avoid using this one. Prefer the multiitem AddOrUpdate where possible. This will not be deprecated though.
        /// </summary>
        /// <typeparam name="TUpperKey"></typeparam>
        /// <typeparam name="TLowerKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="upperKey"></param>
        /// <param name="lowerKey"></param>
        /// <param name="value"></param>
        /// <remarks>Avoid using this where possible. Prefer the multiitem insert methods.</remarks>
        /// <returns></returns>
        public static async Task<bool> TryAdd<TUpperKey, TLowerKey, TValue>(this MultiKeyDictionary<TUpperKey, TLowerKey, TValue> dictionary, TUpperKey upperKey, TLowerKey lowerKey, TValue value) {
            var cts = new CancellationTokenSource();
            var keys = (new[] { (upperKey, lowerKey) }).ToLookup(a => a.upperKey, a => a.lowerKey);

            await dictionary.AddOrUpdateMany(
                keys: keys,
                addValueFactory: (_, __) => value,
                updateValueFactory: (_, __, ___) => {
                    cts.Cancel();
                    return default;
                });

            return !cts.IsCancellationRequested;
        }

        /// <summary>
        /// Avoid using this one. Prefer the multiitem AddOrUpdate where possible. This will not be deprecated though.
        /// </summary>
        /// <typeparam name="TUpperKey"></typeparam>
        /// <typeparam name="TLowerKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="upperKey"></param>
        /// <param name="lowerKey"></param>
        /// <param name="previousValue"></param>
        /// <param name="value"></param>
        /// <remarks>Avoid using this where possible. Prefer the multiitem insert methods.</remarks>
        /// <returns></returns>
        public static async Task<bool> TryUpdate<TUpperKey, TLowerKey, TValue>(this MultiKeyDictionary<TUpperKey, TLowerKey, TValue> dictionary, TUpperKey upperKey, TLowerKey lowerKey, TValue previousValue, TValue value) {
            var cts = new CancellationTokenSource();
            var keys = (new[] { (upperKey, lowerKey) }).ToLookup(a => a.upperKey, a => a.lowerKey);

            await dictionary.AddOrUpdateMany(
                keys: keys,
                addValueFactory: (_, __) => {
                    cts.Cancel();
                    return default;
                },
                updateValueFactory: (_, __, comparedValue) => {
                    if (!previousValue.Equals(previousValue)) {
                        cts.Cancel();
                        return default;
                    }

                    return value;
                });

            return !cts.IsCancellationRequested;
        }

        public static async Task<Option<TValue>> TryGetValue<TUpperKey, TLowerKey, TValue>(this MultiKeyDictionary<TUpperKey, TLowerKey, TValue> dictionary, TUpperKey upperKey, TLowerKey lowerKey, CancellationToken cancellationToken = default) {
            var keys = (new[] { (upperKey, lowerKey) }).ToLookup(a => a.upperKey, a => a.lowerKey);


            var (_, _, result) = await dictionary.GetItems(keys, cancellationToken).FirstOrDefaultAsync();
            return result;
        }
    }
}
