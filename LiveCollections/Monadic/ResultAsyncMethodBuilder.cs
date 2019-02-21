using System.Runtime.CompilerServices;

namespace CSharp.Collections.Monadic.Tasks {
    public class ResultAsyncMethodBuilder<T1, Exception> {
        public Result<T1, Exception> Task { get; private set; } =
            null;

        public static ResultAsyncMethodBuilder<T1, Exception> Create() =>
            new ResultAsyncMethodBuilder<T1, Exception>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
            stateMachine.MoveNext();

        public void SetResult(T1 result) =>
           Task = new Result<T1, Exception>(result);

        public void SetException(Exception ex) {
            Task = new Result<T1, Exception>(ex);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine) { }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine {
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine {
        }
    }
}
