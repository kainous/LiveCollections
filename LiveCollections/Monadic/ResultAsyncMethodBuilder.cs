using System;
using System.Runtime.CompilerServices;

namespace CSharp.Collections.Monadic.Tasks {
    public class ResultAsyncMethodBuilder<T> {
        public Result<T> Task { get; private set; } =
            null;

        public static ResultAsyncMethodBuilder<T> Create() =>
            new ResultAsyncMethodBuilder<T>();

        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
            stateMachine.MoveNext();

        public void SetResult(T result) =>
            Task = new Result<T>(result);

        public void SetException(Exception ex) {
            Task = new Result<T>(ex);
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
