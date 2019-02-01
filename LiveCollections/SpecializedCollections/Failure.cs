namespace System.Collections.Monadic {
    // Not yet tested
    internal sealed class Failure<TSuccess, TFailure> : Result<TSuccess, TFailure> {
        public TFailure Value { get; }
        public Failure(TFailure error) {
            Value = error;
        }
    }
}
