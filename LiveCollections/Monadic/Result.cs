using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CSharp.Collections.Monadic.Tasks;

namespace CSharp.Collections.Monadic {
    [AsyncMethodBuilder(typeof(ResultAsyncMethodBuilder<>))]
    public class Result<T> : Choice<T, Exception>, IEquatable<Result<T>> {
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

        public static Result<T> operator |(Result<T> x, Result<T> y) =>
            x.If(x, y);

        public static T operator |(Result<T> x, T y) =>
            x.If(a => a, _ => y);
    }

    namespace Tasks {
        public static class ResultExtensions {
            public static ResultAwaiter<T> GetAwaiter<T>(this Result<T> result) =>
                new ResultAwaiter<T>(result);
        }
    }    

    public static class Result {
        public static Result<T> Try<T>(Func<T> action, Action @finally) {
            try {
                return new Result<T>(action());
            }
            catch (Exception ex) {
                return new Result<T>(ex);
            }
            finally {
                @finally();
            }
        }

        public static T Run<T>(this Choice<T, Exception> result) =>
            result.If(a => a, ExceptionHelpers.Rethrow<T>);

        public static Result<T> Try<T>(Func<T> action) =>
            Try(action, () => { });

        public static Result<TResult> Bind<TSource, TResult>(this Result<TSource> source, Func<TSource, Result<TResult>> binder) =>
            source.IsFirst ? binder(source.Value1) : new Result<TResult>(source.Value2);

        public static Result<T> SelectMany<T>(this Result<Result<T>> source) =>
            source.Bind(a => a);        

        public static Result<TResult> SelectMany<TSource, TIntermediate, TResult>(this Result<TSource> source, Func<TSource, Result<TIntermediate>> intermediateSelector, Func<TSource, TIntermediate, TResult> resultSelector) =>
            source.Bind(x => intermediateSelector(x).Bind(y => new Result<TResult>(resultSelector(x, y))));        

        public static Result<TResult> SelectMany<TSource, TResult>(this Result<TSource> source, Func<TSource, Result<TResult>> resultSelector) =>
            source.Bind(resultSelector);        

        public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector1, Func<Exception, Exception> selector2) =>
            source.IsFirst
                ? new Result<TResult>(selector1(source.Value1))
                : new Result<TResult>(selector2(source.Value2));

        public static Result<TResult> Select<TSource, TResult>(this Result<TSource> source, Func<TSource, TResult> selector) =>
            source.Select(selector, a => a);

        public static Result<T> SelectError<T>(this Result<T> source, Func<Exception, Exception> selector) =>
            source.Select(a => a, selector);

        public static Option<T> ToOption<T>(this Result<T> result) =>
            result.If(Option.Some, _ => Option.None<T>());

        public static IEnumerable<T> ToEnumerable<T>(this Result<T> result) =>
            result.If(a => Enumerable.Repeat(a, 1), _ => Enumerable.Empty<T>());

        public static Result<T> Success<T>(this T value) =>
            new Result<T>(value);        

        public static Result<T> Failure<T>(this Exception exception) =>
            new Result<T>(exception);
    }    
}
