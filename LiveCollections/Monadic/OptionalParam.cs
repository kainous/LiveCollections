using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Collections.Monadic {
    public struct OptionalParam<T> {
        private readonly bool _hasValue;
        private readonly T _value;

        public OptionalParam(T value) {
            _hasValue = true;
            _value = value;
        }

        public static implicit operator OptionalParam<T>(T value) => 
            new OptionalParam<T>(value);

        public static T operator |(OptionalParam<T> optional, T alternativeValue) => 
            optional._hasValue ? optional._value : alternativeValue;
    }
}
