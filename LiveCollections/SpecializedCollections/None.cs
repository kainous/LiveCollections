using System.Collections.Generic;

namespace System.Collections.Monadic {
    public sealed class None<T> : Option<T> {
        public None() {
        }

        public override IEnumerable<T> AsEnumerable() {
            yield break;
        }
    }
}
