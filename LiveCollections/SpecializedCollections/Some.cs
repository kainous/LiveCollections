using System.Collections.Generic;

namespace Halliburton.IC.SpecializedCollections {
    public sealed class Some<T> : Option<T> {
        public T Value { get; }
        public Some(T value) {
            Value = value;
        }

        public override IEnumerable<T> AsEnumerable() {
            yield return Value;
        }
    }
}
