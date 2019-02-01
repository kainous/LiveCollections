using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpecializedCollections {
    public interface ICollectionViewer<T> {
        long Count { get; }
        IEnumerable<T> AsEnumerable();
    }

    public interface ICollectionAdder<T> : ICollectionViewer<T> {
        IObservable<Option<T, Exception>> Add(IEnumerable<T> items, Func<T, T, Exception, Option<T, Exception>> fallbackAction, CancellationToken token = default);
    }

    public interface ICollectionRemover<T> : ICollectionViewer<T> {
        Task Clear();
        IObservable<Option<T, Exception>> Remove(IEnumerable<T> items);
    }

    public static class TransactedCollection {
        public static async Task AddAsync<T>(this ICollectionAdder<T> collection, IEnumerable<T> items, CancellationToken token = default) {
            if (items?.Any() == true) {
                await collection.Add(items, (_, __, ex) => throw ex, token);
            }
        }

        public static void Add<T>(this ICollectionAdder<T> collection, IEnumerable<T> items, CancellationToken token = default) {
            collection.AddAsync(items, token).RunSynchronously();
        }

        public static void Add<T>(this ICollectionAdder<T> collection, params T[] items) {
            collection.Add((IEnumerable<T>)items);
        }

        public static async Task AddWithResume<T>(this ICollectionAdder<T> collection, IEnumerable<T> items, CancellationToken token = default) {
            if (items?.Any() != true) {
                return;
            }

            await collection.Add(items, (_, __, ex) => new Failure<T, Exception>(ex), token);
        }

        public static async Task AddWithThrow<T>(this ICollectionAdder<T> collection, IEnumerable<T> items, CancellationToken token = default) {
            if (items?.Any() != true) {
                return;
            }

            await collection.Add(items, (_, __, ex) => throw ex, token);
        }
    }

    public interface IIndexedCollectionView<TIndex, TValue> : ICollectionViewer<TValue> {
        Task<TValue> this[TIndex index] { get; }
        Task<TIndex> FindFirst(Func<TValue, bool> predicate);
        Task<TIndex> FindLast(Func<TValue, bool> predicate);
        IObservable<(TIndex, TValue)> FindAll(Func<TValue, bool> predicate);
    }
}
