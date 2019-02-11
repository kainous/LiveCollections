namespace Halliburton.IC.SpecializedCollections {
    // TODO Test this
    internal sealed class Success<TSuccess, TFailure> : Result<TSuccess, TFailure> {
        public TSuccess Value { get; }
        public Success(TSuccess value) {
            Value = value;
        }
    }
}
