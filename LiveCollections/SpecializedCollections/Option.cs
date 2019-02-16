using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Halliburton.IC.SpecializedCollections {
    public abstract class Option<T> {
        internal Option() { }
        public abstract IEnumerable<T> AsEnumerable();
    }

    public static class Option {
        public static Option<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
            dictionary.TryGetValue(key, out var result)
            ? result.AsOption()
            : None<TValue>();

        public static Option<T> AsOption<T>(this T value) =>
            new Some<T>(value);

        public static Option<T> None<T>() =>
            new None<T>();

        public static Option<R> Bind<T, R>(this Option<T> option, Func<T, Option<R>> binding) {
            switch (option) {
                case Some<T> a:
                    return binding(a.Value);
                default:
                    return None<R>();
            }
        }

        public static Option<R> Select<T, R>(this Option<T> option, Func<T, R> mapping) =>
            option.Bind(a => mapping(a).AsOption());

        public static bool Any<T>(this Option<T> option, Func<T, bool> predicate) {
            switch (option) {
                case Some<T> a when predicate(a.Value):
                    return true;
                default:
                    return false;
            }
        }

        public static bool Any<T>(this Option<T> option, T comparison, IEqualityComparer<T> comparer = null) =>
            option.Any(a => (comparer ?? EqualityComparer<T>.Default).Equals(a, comparison));

        public static bool Any<T>(this Option<T> option) =>
            option.Any(_ => true);

        public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
            option.Any(predicate) ? option : None<T>();

        public static void ForEach<T>(this Option<T> option, Action<T> action) {
            switch (option) {
                case Some<T> a:
                    action(a.Value);
                    break;
            }
        }

        public static async Task ForEachAsync<T>(this Option<T> option, Func<T, Task> action) {
            switch (option) {
                case Some<T> a:
                    await action(a.Value);
                    break;
            }
        }

        public static Option<T> FirstOrNone<T>(this IEnumerable<Option<T>> options, Func<T, bool> predicate) {
            foreach (var item in options) {
                switch (item) {
                    case Some<T> i when predicate(i.Value):
                        return item;
                }
            }

            return None<T>();
        }

        public static Option<T> FirstOrNone<T>(this IEnumerable<Option<T>> options) {
            return options.FirstOrNone(_ => true);
        }

        public static T FirstOrDefault<T>(this IEnumerable<Option<T>> options, Func<T, bool> predicate, T defaultValue = default) {
            switch (options.FirstOrNone(predicate)) {
                case Some<T> item:
                    return item.Value;
            }

            return defaultValue;
        }

        public static T FirsOrDefault<T>(this IEnumerable<Option<T>> options, T defaultValue = default) {
            return options.FirstOrDefault(_ => true, defaultValue);
        }

        public static T First<T>(this IEnumerable<Option<T>> options, Func<T, bool> predicate) {
            switch (options.FirstOrNone(predicate)) {
                case Some<T> item:
                    return item.Value;
            }

            throw new IndexOutOfRangeException("No item in the enumeration matched");
        }

        public static T First<T>(this IEnumerable<Option<T>> options) {
            return options.First(_ => true);
        }

        public static Option<TResult> SelectMany<TSource, TCollection, TResult>(this Option<TSource> source, Func<TSource, Option<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            switch (source) {
                case Some<TSource> outer:
                    var middle = collectionSelector(outer.Value);
                    switch (middle) {
                        case Some<TCollection> b:
                            return resultSelector(outer.Value, b.Value).AsOption();
                    }
                    break;
            }
            return None<TResult>();

        }

        public static Option<TResult> SelectMany<TSource, TResult>(this Option<TSource> source, Func<TSource, Option<TResult>> selector) {
            switch (source) {
                case Some<TSource> outer:
                    return selector(outer.Value);
            }

            return None<TResult>();
        }

        public static Option<T> SelectMany<T>(this Option<Option<T>> source) {
            switch (source) {
                case Some<Option<T>> result:
                    return result.Value;
            }

            return None<T>();
        }

        public static IEnumerable<T> SelectMany<T>(this IEnumerable<Option<T>> source) {
            foreach (var item in source) {
                switch (item) {
                    case Some<T> result:
                        yield return result.Value;
                        break;
                }
            }
        }

        public static IObservable<T> SelectMany<T>(this IObservable<Option<T>> source) {
            return from item in source
                   let a = item as Some<T>
                   where a != null
                   select a.Value;
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, Option<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            foreach (var item in source) {
                var collection = collectionSelector(item);
                switch (collection) {
                    case Some<TCollection> some:
                        yield return resultSelector(item, some.Value);
                        break;
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this Option<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) {
            switch (source) {
                case Some<IEnumerable<TSource>> result:
                    return result.Value.SelectMany(collectionSelector, resultSelector);
            }
            return Enumerable.Empty<TResult>();
        }

        public static IEnumerable<T> SelectMany<T>(this Option<IEnumerable<T>> source) {
            switch (source) {
                case Some<IEnumerable<T>> result:
                    return result.Value;
            }

            return Enumerable.Empty<T>();
        }

        public static IObservable<T> SelectMany<T>(this Option<IObservable<T>> source) {
            switch (source) {
                case Some<IObservable<T>> result:
                    return result.Value;
            }

            return Observable.Empty<T>();
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this Option<TSource> option, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) {
            var acc = seed;
            switch (option) {
                case Some<TSource> result:
                    acc = func(acc, result.Value);
                    break;
            }

            return resultSelector(acc);
        }

        public static TResult Aggregate<TSource, TResult>(this Option<TSource> option, TResult seed, Func<TResult, TSource, TResult> func) {
            var acc = seed;
            switch (option) {
                case Some<TSource> source:
                    acc = func(acc, source.Value);
                    break;
            }

            return acc;
        }

        public static bool Contains<T>(this Option<T> option, T comparand, IEqualityComparer<T> comparer = null) {
            if (comparer == null) {
                comparer = EqualityComparer<T>.Default;
            }

            switch (option) {
                case Some<T> result when comparer.Equals(comparand, result.Value):
                    return true;
            }

            return false;
        }

        // Not yet tested
        // TODO add this back in
        internal static Result<TSuccess, EmptyTuple> AsResult<TSuccess>(this Option<TSuccess> option) {
            switch (option) {
                case Some<TSuccess> some:
                    return new Success<TSuccess, EmptyTuple>(some.Value);
                default:
                    return new Failure<TSuccess, EmptyTuple>(EmptyTuple.Value);
            }
        }
    }
}
