namespace Dashing.CodeGeneration {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal interface IEnumerableAwaiter<out T> : INotifyCompletion {
        IEnumerableAwaiter<T> GetAwaiter();

        bool IsCompleted { get; }

        IEnumerable<T> GetResult();
    }
}