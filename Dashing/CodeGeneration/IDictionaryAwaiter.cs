namespace Dashing.CodeGeneration {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal interface IDictionaryAwaiter<out T> : INotifyCompletion {
        IDictionaryAwaiter<T> GetAwaiter();

        bool IsCompleted { get; }

        IDictionary GetResult();
    }
}