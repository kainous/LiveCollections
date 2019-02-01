using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpecializedCollections {
    public abstract class Option<TSuccess, TFailure> {
        internal Option() { }
    }

    public abstract class Option<T> {
        internal Option() { }
        public abstract IEnumerable<T> AsEnumerable();
    }

    public static class Option {
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

        public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) {
            switch (option) {
                case Some<T> a when predicate(a.Value):
                    return option;
                default:
                    return None<T>();
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
            switch(options.FirstOrNone(predicate)) {
                case Some<T> item:
                    return item.Value;
            }

            return defaultValue;
        }

        public static T FirsOrDefault<T>(this IEnumerable<Option<T>> options, T defaultValue = default) {
            return options.FirstOrDefault(_ => true, defaultValue);
        }

        public static T First<T> (this IEnumerable<Option<T>> options, Func<T, bool> predicate) {
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
    }
}
