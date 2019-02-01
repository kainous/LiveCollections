using System.Collections.Generic;

namespace System.Collections.Monadic {
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
