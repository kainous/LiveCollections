using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CSharp.Collections.Monadic.Tasks;

namespace CSharp.Collections.Monadic {
    public class Result<T1, T2> : IEquatable<Result<T1, T2>> {
        internal bool IsSuccess { get; }
        internal T1 Value1 { get; }
        internal T2 Value2 { get; }

        public Result(T1 value) {
            Value1 = value;
            IsSuccess = true;
        }

        public Result(T2 value) {
            Value2 = value;
            IsSuccess = false;
        }

        public Result<T2, T1> Swap() =>
            IsSuccess ? new Result<T2, T1>(Value1) : new Result<T2, T1>(Value2);

        public void If(Action<T1> action1, Action<T2> action2) {
            if (IsSuccess) {
                action1(Value1);
            }
            else {
                action2(Value2);
            }
        }

        public T If<T>(Func<T1, T> action1, Func<T2, T> action2) =>
            IsSuccess ? action1(Value1) : action2(Value2);

        public T If<T>(T result1, T result2) =>
            IsSuccess ? result1 : result2;

        public bool TryGet(out T1 result, T1 alternate = default) {
            if (IsSuccess) {
                result = Value1;
                return true;
            }
            else {
                result = alternate;
                return false;
            }
        }

        public bool TryGet(out T2 result, T2 alternate = default) {
            if (IsSuccess) {
                result = alternate;
                return false;
            }
            else {
                result = Value2;
                return true;
            }
        }

        public override string ToString() =>
            IsSuccess ? $"Success: {Value1}" : $"Failure: {Value2}";

        protected bool EqualsCore(Result<T1, T2> other) => 
            !Equals(other, null)
                && (other.IsSuccess && Equals(Value1, other.Value1) || Equals(Value2, other.Value2));

        public static bool operator ==(Result<T1, T2> x, Result<T1, T2> y) =>
            ReferenceEquals(x, y) || !Equals(x, null) && x.EqualsCore(y);

        public static bool operator !=(Result<T1, T2> x, Result<T1, T2> y) =>
            !(x == y);

        public bool Equals(Result<T1, T2> other) =>
            ReferenceEquals(this, other) || EqualsCore(other);

        public override bool Equals(object obj) => 
            ReferenceEquals(this, obj) || obj is Result<T1, T2> other && EqualsCore(other);

        public override int GetHashCode() => 
            IsSuccess 
            ? Value1.GetHashCode() 
            : Value2.GetHashCode();
    }

    namespace Tasks {
        public static class ResultExtensions {
            public static ResultAwaiter<T1, T2> GetAwaiter<T1, T2>(this Result<T1, T2> result) =>
                new ResultAwaiter<T1, T2>(result);
        }
    }

    public static class Result {
        private static T Id<T>(T value) =>
            value;

        private static void Id() { }

        public static Result<T, Exception> Try<T>(Func<T> action, Action @finally) {
            try {
                return new Result<T, Exception>(action());
            }
            catch (Exception ex) {
                return new Result<T, Exception>(ex);
            }
            finally {
                @finally();
            }
        }

        public static T Run<T>(this Result<T, Exception> result) =>
            result.If(Id, ExceptionHelpers.Rethrow<T>);

        public static Result<T, Exception> Try<T>(Func<T> action) =>
            Try(action, Id);

        public static Result<TResult, T2> Bind<TSource, TResult, T2>(this Result<TSource, T2> source, Func<TSource, Result<TResult, T2>> binder) =>
            source.IsSuccess ? binder(source.Value1) : new Result<TResult, T2>(source.Value2);

        public static Result<TResult> Bind<TSource, TResult>(this Result<TSource> source, Func<TSource, Result<TResult>> binder) =>
            source.IsSuccess ? binder(source.Value1) : new Result<TResult>(source.Value2);

        public static Result<T1, T2> SelectMany<T1, T2>(this Result<Result<T1, T2>, T2> source) =>
            source.Bind(Id);

        public static Result<T> SelectMany<T>(this Result<Result<T>> source) =>
            source.Bind(Id);

        public static Result<TResult, T2> SelectMany<TSource, TIntermediate, TResult, T2>(this Result<TSource, T2> source, Func<TSource, Result<TIntermediate, T2>> intermediateSelector, Func<TSource, TIntermediate, TResult> resultSelector) =>
            source.Bind(x => intermediateSelector(x).Bind(y => new Result<TResult, T2>(resultSelector(x, y))));

        public static Result<TResult> SelectMany<TSource, TIntermediate, TResult>(this Result<TSource> source, Func<TSource, Result<TIntermediate>> intermediateSelector, Func<TSource, TIntermediate, TResult> resultSelector) =>
            source.Bind(x => intermediateSelector(x).Bind(y => new Result<TResult>(resultSelector(x, y))));

        public static Result<TResult, T2> SelectMany<TSource, TResult, T2>(this Result<TSource, T2> source, Func<TSource, Result<TResult, T2>> resultSelector) =>
            source.Bind(resultSelector);

        public static Result<TResult> SelectMany<TSource, TResult>(this Result<TSource> source, Func<TSource, Result<TResult>> resultSelector) =>
            source.Bind(resultSelector);

        public static Result<TResult1, TResult2> Select<TSource1, TResult1, TSource2, TResult2>(this Result<TSource1, TSource2> source, Func<TSource1, TResult1> selector1, Func<TSource2, TResult2> selector2) =>
            source.IsSuccess
                ? new Result<TResult1, TResult2>(selector1(source.Value1))
                : new Result<TResult1, TResult2>(selector2(source.Value2));

        public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector1, Func<Exception, Exception> selector2) =>
            source.IsSuccess
                ? new Result<TResult>(selector1(source.Value1))
                : new Result<TResult>(selector2(source.Value2));

        public static Result<TResult, T2> Select<TSource, TResult, T2>(this Result<TSource, T2> source, Func<TSource, TResult> selector) =>
            source.Select(selector, Id);

        public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector) =>
            source.Select(selector, Id);

        public static Result<T1, TResult> SelectError<T1, TSource, TResult>(this Result<T1, TSource> source, Func<TSource, TResult> selector) =>
            source.Select(Id, selector);

        public static Result<T> SelectError<T>(this Result<T> source, Func<Exception, Exception> selector) =>
            source.Select(Id, selector);

        public static Option<T1> ToOption<T1, T2>(this Result<T1, T2> result) =>
            result.If(Option.Some, _ => Option.None<T1>());

        public static IEnumerable<T1> ToEnumerable<T1, T2>(this Result<T1, T2> result) =>
            result.If(a => Enumerable.Repeat(a, 1), _ => Enumerable.Empty<T1>());

        public static Result<T> Success<T>(this T value) =>
            new Result<T>(value);

        public static Result<T1, T2> Success<T1, T2>(this T1 value) =>
            new Result<T1, T2>(value);

        public static Result<T1, T2> Failure<T1, T2>(this T2 value) =>
            new Result<T1, T2>(value);

        public static Result<T> Failure<T>(this Exception exception) =>
            new Result<T>(exception);
    }

    [AsyncMethodBuilder(typeof(ResultAsyncMethodBuilder<>))]
    public class Result<T> : Result<T, Exception> , IEquatable<Result<T>> {
        public Result(T value) : base(value) { }
        public Result(Exception exception) : base(exception) { }

        public static bool operator ==(Result<T> x, Result<T> y) =>
            ReferenceEquals(x, y) || !Equals(x, null) && x.EqualsCore(y);

        public static bool operator !=(Result<T> x, Result<T> y) =>
            !(x == y);

        public bool Equals(Result<T> other) =>
            ReferenceEquals(this, other) || EqualsCore(other);

        // Required because of warnings
        public override bool Equals(object obj) =>
            base.Equals(obj);

        // Required because of warnings
        public override int GetHashCode() =>
            base.GetHashCode();
    }
}
