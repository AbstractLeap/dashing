namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class FetchCollectionAwaiter<T> : IEnumerableAwaiter<T> {
        public TaskAwaiter<IEnumerable<T>> Awaiter { get; set; }

        public IEnumerable<T> Results { get; set; }

        public IEnumerableAwaiter<T> GetAwaiter() {
            return this;
        }

        public bool IsCompleted
        {
            get
            {
                return this.Awaiter.IsCompleted;
            }
        }

        public IEnumerable<T> GetResult() {
            return this.Results;
        }

        public void OnCompleted(Action continuation) {
            this.Awaiter.OnCompleted(continuation);
        }
    }
}