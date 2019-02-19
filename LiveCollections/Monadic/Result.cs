using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace CSharp.Collections.Monadic {
    public sealed class Result<T1, T2> {
        internal bool IsSuccess { get; }
        internal T1 Value1 { get; }
        internal T2 Value2 { get; }

        internal Result(T1 value) {
            Value1 = value;
            IsSuccess = true;
        }

        internal Result(T2 value) {
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
    }

    public static class Result {
        private static T id<T>(T value) =>
            value;

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
            result.If(a => a, ex => { ExceptionDispatchInfo.Capture(ex).Throw(); throw ex; });

        public static Result<T, Exception> Try<T>(Func<T> action) =>
            Try(action, () => { });

        public static Result<TResult, T2> Bind<TSource, TResult, T2>(this Result<TSource, T2> source, Func<TSource, Result<TResult, T2>> binder) =>
            source.IsSuccess ? binder(source.Value1) : new Result<TResult, T2>(source.Value2);

        public static Result<T1, T2> SelectMany<T1, T2>(this Result<Result<T1, T2>, T2> source) =>
            source.Bind(id);

        public static Result<TResult, T2> SelectMany<TSource, TIntermediate, TResult, T2>(this Result<TSource, T2> source, Func<TSource, Result<TIntermediate, T2>> intermediateSelector, Func<TSource, TIntermediate, TResult> resultSelector) =>
            source.Bind(x => intermediateSelector(x).Bind(y => new Result<TResult, T2>(resultSelector(x, y))));

        public static Result<TResult, T2> SelectMany<TSource, TResult, T2>(this Result<TSource, T2> source, Func<TSource, Result<TResult, T2>> resultSelector) =>
            source.Bind(x => resultSelector(x));

        public static Option<T1> ToOption<T1, T2>(this Result<T1, T2> result) => 
            result.If(a => a.Some(), _ => Option.None<T1>());

        public static IEnumerable<T1> AsEnumerable<T1, T2>(this Result<T1, T2> result) =>
            result.If(a => Enumerable.Repeat(a, 1), _ => Enumerable.Empty<T1>());
    }
}
