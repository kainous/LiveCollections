using System;
using System.Runtime.CompilerServices;

namespace CSharp.Collections.Monadic.Tasks {
    public class ResultAwaiter<T> : INotifyCompletion {
        private Result<T> _result;

        public ResultAwaiter(Result<T> result) {
            _result = result;
        }

        public bool IsCompleted =>
            _result.IsFirst;

        public void OnCompleted(Action continuation) {
            // Only called when _option is None
        }

        // Only called when _option is Some
        public T GetResult() =>
            _result.Value1;
    }
}
