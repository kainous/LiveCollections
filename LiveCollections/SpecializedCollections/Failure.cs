namespace Halliburton.IC.SpecializedCollections {
    // Not yet tested
    internal sealed class Failure<TSuccess, TFailure> : Result<TSuccess, TFailure> {
        public TFailure Value { get; }
        public Failure(TFailure error) {
            Value = error;
        }
    }
}
