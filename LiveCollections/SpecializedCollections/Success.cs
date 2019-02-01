namespace System.Collections.Monadic {
    // TODO Test this
    internal sealed class Success<TSuccess, TFailure> : Result<TSuccess, TFailure> {
        public TSuccess Value { get; }
        public Success(TSuccess value) {
            Value = value;
        }
    }
}
