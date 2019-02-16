using System.Collections.Generic;

namespace SpecializedCollections {
    public sealed class None<T> : Option<T> {
        public None() {
        }

        public override IEnumerable<T> AsEnumerable() {
            yield break;
        }
    }
}
