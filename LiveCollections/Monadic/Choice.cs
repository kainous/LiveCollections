using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Collections.Monadic {
    public class Choice<T1, T2> : IEquatable<Choice<T1, T2>> {
        internal bool IsFirst { get; }
        internal T1 Value1 { get; }
        internal T2 Value2 { get; }

        public Choice(T1 value) {
            Value1 = value;
            IsFirst = true;
        }

        public Choice(T2 value) {
            Value2 = value;
            IsFirst = false;
        }

        public Choice<T2, T1> Swap() =>
            IsFirst ? new Choice<T2, T1>(Value1) : new Choice<T2, T1>(Value2);

        public void If(Action<T1> action1, Action<T2> action2) {
            if (IsFirst) {
                action1(Value1);
            }
            else {
                action2(Value2);
            }
        }

        public T If<T>(Func<T1, T> action1, Func<T2, T> action2) =>
            IsFirst ? action1(Value1) : action2(Value2);

        public T If<T>(T result1, T result2) =>
            IsFirst ? result1 : result2;

        public bool TryGet(out T1 result, T1 alternate = default) {
            if (IsFirst) {
                result = Value1;
                return true;
            }
            else {
                result = alternate;
                return false;
            }
        }

        public bool TryGet(out T2 result, T2 alternate = default) {
            if (IsFirst) {
                result = alternate;
                return false;
            }
            else {
                result = Value2;
                return true;
            }
        }

        public override string ToString() =>
            IsFirst ? $"First: {Value1}" : $"Second: {Value2}";

        protected bool EqualsCore(Choice<T1, T2> other) =>
            !Equals(other, null)
                && (other.IsFirst && Equals(Value1, other.Value1) || Equals(Value2, other.Value2));

        public static bool operator ==(Choice<T1, T2> x, Choice<T1, T2> y) =>
            ReferenceEquals(x, y) || !Equals(x, null) && x.EqualsCore(y);

        public static bool operator !=(Choice<T1, T2> x, Choice<T1, T2> y) =>
            !(x == y);

        public bool Equals(Choice<T1, T2> other) =>
            ReferenceEquals(this, other) || EqualsCore(other);

        public override bool Equals(object obj) =>
            ReferenceEquals(this, obj) || obj is Choice<T1, T2> other && EqualsCore(other);

        public override int GetHashCode() =>
            IsFirst
            ? Value1.GetHashCode()
            : Value2.GetHashCode();
    }

    public static class Choice {
        public static Choice<T1, T2> FirstChoiceOf<T1, T2>(this T1 value) =>
            new Choice<T1, T2>(value);

        public static Choice<T1, T2> SecondChoiceOf<T1, T2>(this T2 value) =>
            new Choice<T1, T2>(value);
    }

    namespace Linq {
        public static class Choice {
            public static Choice<TResult, T2> Bind<TSource, TResult, T2>(this Choice<TSource, T2> source, Func<TSource, Choice<TResult, T2>> binder) =>
                source.IsFirst ? binder(source.Value1) : new Choice<TResult, T2>(source.Value2);

            public static Choice<T1, T2> SelectMany<T1, T2>(this Choice<Choice<T1, T2>, T2> source) =>
                source.Bind(a => a);

            public static Choice<TResult, T2> SelectMany<TSource, TIntermediate, TResult, T2>(this Choice<TSource, T2> source, Func<TSource, Choice<TIntermediate, T2>> intermediateSelector, Func<TSource, TIntermediate, TResult> resultSelector) =>
                source.Bind(x => intermediateSelector(x).Bind(y => new Choice<TResult, T2>(resultSelector(x, y))));

            public static Choice<TResult, T2> SelectMany<TSource, TResult, T2>(this Choice<TSource, T2> source, Func<TSource, Choice<TResult, T2>> resultSelector) =>
                source.Bind(resultSelector);

            public static Choice<TResult1, TResult2> Select<TSource1, TResult1, TSource2, TResult2>(this Choice<TSource1, TSource2> source, Func<TSource1, TResult1> selector1, Func<TSource2, TResult2> selector2) =>
                source.IsFirst
                    ? new Choice<TResult1, TResult2>(selector1(source.Value1))
                    : new Choice<TResult1, TResult2>(selector2(source.Value2));

            public static Choice<TResult, T2> Select<TSource, TResult, T2>(this Choice<TSource, T2> source, Func<TSource, TResult> selector) =>
                source.Select(selector, a => a);

            public static Choice<T1, TResult> SelectSecond<T1, TSource, TResult>(this Choice<T1, TSource> source, Func<TSource, TResult> selector) =>
                source.Select(a => a, selector);
        }
    }
}
