using System;
using System.Runtime.CompilerServices;

namespace CSharp.Collections.Monadic.Tasks {
    public class OptionAwaiter<T> : INotifyCompletion {
        private Option<T> _option;

        public OptionAwaiter(Option<T> option) => 
            _option = option;

        public bool IsCompleted =>
            _option.HasValue;

        public void OnCompleted(Action continuation) {
            // Only called when _option is None
        }

        // Only called when _option is Some
        public T GetResult() =>
            _option.Value;
    }
}
