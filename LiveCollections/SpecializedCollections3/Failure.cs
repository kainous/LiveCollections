namespace SpecializedCollections {
    public sealed class Failure<TSuccess, TFailure> : Option<TSuccess, TFailure> {
        public TFailure Value { get; }
        public Failure(TFailure error) {
            Value = error;
        }
    }
}
