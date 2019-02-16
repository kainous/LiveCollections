namespace SpecializedCollections {
    public sealed class Success<TSuccess, TFailure> : Option<TSuccess, TFailure> {
        public TSuccess Value { get; }
        public Success(TSuccess value) {
            Value = value;
        }
    }
}
