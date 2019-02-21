using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CSharp.Collections.Monadic.Tasks;

namespace CSharp.Collections.Monadic {
    // Works similar to Nullable<T> without the class requirement
    // Also works with LINQ
    [AsyncMethodBuilder(typeof(OptionAsyncMethodBuilder<>))]
    public struct Option<T> : IEquatable<Option<T>> {
        internal bool HasValue { get; }
        internal T Value { get; }
        internal Option(T value) {
            HasValue = true;
            Value = value;
        }

        internal static Option<T> NoneValue { get; } = new Option<T>();

        public void If(Action<T> action, Action alternateAction) {
            if (HasValue) {
                action(Value);
            }
            else {
                alternateAction();
            }
        }

        public bool TryGet(out T value) {
            value = HasValue ? Value : default(T);
            return HasValue;
        }

        public void If(Action<T> action) {
            if (HasValue) {
                action(Value);
            }
        }

        public static bool operator ==(Option<T> first, Option<T> second) => 
            !first.HasValue ? !second.HasValue : first.Value.Equals(second.Value);

        public R If<R>(Func<T, R> action, Func<R> alternateAction) =>
            HasValue ? action(Value) : alternateAction();

        public R If<R>(Func<T, R> action, R alternateAction) =>
            HasValue ? action(Value) : alternateAction;

        public T GetValue(T alternativeValue = default(T)) =>
            HasValue ? Value : alternativeValue;

        public Option<R> Bind<R>(Func<T, Option<R>> binder) =>
            HasValue ? binder(Value) : Option<R>.NoneValue;

        public override string ToString() => 
            HasValue ? $"Some({Value})" : "None";

        public static bool operator !=(Option<T> first, Option<T> second) => 
            !(first == second);

        public bool Equals(Option<T> second) => 
            this == second;

        public override bool Equals(object obj) => 
            obj is Option<T> some ? this == some : false;

        public override int GetHashCode() => 
            HasValue ? Value.GetHashCode() : 0;
    }

    public static class Option {
        public static Option<T> Some<T>(this T value) {
            return new Option<T>(value);
        }

        public static Option<T> None<T>() {
            return Option<T>.NoneValue;
        }
    }

    namespace Tasks {
        public static class OptionExtensions {
            public static OptionAwaiter<T> GetAwaiter<T>(this Option<T> option) =>
                new OptionAwaiter<T>(option);
        }
    }

    namespace Linq {
        public static class OptionExtensions {
            public static Option<TResult> Select<TSource, TResult>(this Option<TSource> source, Func<TSource, TResult> selector) =>
                source.Bind(x => selector(x).Some());

            public static Option<T> SelectMany<T>(this Option<Option<T>> source) =>
                source.Bind(x => x);

            public static Option<TResult> SelectMany<TResult, TIntermediate, TSource>(this Option<TSource> source, Func<TSource, Option<TIntermediate>> optionSelector, Func<TSource, TIntermediate, TResult> resultSelector) =>
                source.Bind(x => optionSelector(x).Bind(y => resultSelector(x, y).Some()));

            public static Option<TResult> SelectMany<TResult, TSource>(this Option<TSource> source, Func<TSource, Option<TResult>> resultSelector) =>
                source.Bind(x => resultSelector(x));

            public static IEnumerable<T> SelectMany<T>(this Option<IEnumerable<T>> source) =>
                source.If(a => a, Enumerable.Empty<T>());

            public static IEnumerable<T> SelectMany<T>(this IEnumerable<Option<T>> source) =>
                source.SelectMany(a => a.If(b => Enumerable.Repeat(b, 1), Enumerable.Empty<T>()));

            public static Option<T> Where<T>(this Option<T> source, Func<T, bool> predicate) =>
                source.Bind(a => predicate(a) ? source : Option<T>.NoneValue);

            public static IEnumerable<T> AsEnumerable<T>(this Option<T> source) =>
                source.If(a => Enumerable.Repeat(a, 1), () => Enumerable.Empty<T>());
        }
    }
}
