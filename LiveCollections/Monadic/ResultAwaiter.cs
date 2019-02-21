using System;
using System.Runtime.CompilerServices;

namespace CSharp.Collections.Monadic.Tasks {
    public class ResultAwaiter<T1, T2> : INotifyCompletion {
        private Result<T1, T2> _result;

        public ResultAwaiter(Result<T1, T2> result) {
            _result = result;
        }

        public bool IsCompleted =>
            _result.IsSuccess;

        public void OnCompleted(Action continuation) {
            // Only called when _option is None
        }

        // Only called when _option is Some
        public T1 GetResult() =>
            _result.Value1;
    }
}
