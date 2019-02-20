using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace CSharp.Collections.Monadic {
    // Works similar to Nullable<T> without the class requirement
    // Also works with LINQ
    [AsyncMethodBuilder(typeof(OptionAsyncMethodBuilder<>))]
    [DebuggerDisplay("{HasValue ? \"Some(\" + Value.ToString() + \")\" : None}")]
    public sealed class Option<T> {
        internal bool HasValue { get; }
        internal T Value { get; }
        internal Option(T value) {
            HasValue = true;
            Value = value;
        }

        private Option() { }
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
            value = HasValue ? Value : default;
            return HasValue;
        }

        public void If(Action<T> action) {
            if (HasValue) {
                action(Value);
            }
        }

        public R If<R>(Func<T, R> action, Func<R> alternateAction) =>
            HasValue ? action(Value) : alternateAction();

        public R If<R>(Func<T, R> action, R alternateAction) =>
            HasValue ? action(Value) : alternateAction;

        public T GetValue(T alternativeValue = default) =>
            HasValue ? Value : alternativeValue;

        public Option<R> Bind<R>(Func<T, Option<R>> binder) =>
            HasValue ? binder(Value) : Option<R>.NoneValue;
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
        public static OptionAwaiter<T> GetAwaiter<T>(this Option<T> option) =>
            new OptionAwaiter<T>(option);
    }

    namespace Linq {
        public static class Option {
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
