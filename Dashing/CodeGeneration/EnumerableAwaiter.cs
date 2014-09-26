namespace Dashing.CodeGeneration {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class EnumerableAwaiter<T> : IEnumerableAwaiter<T> {
        public TaskAwaiter<IEnumerable<T>> Awaiter { get; set; }

        public void OnCompleted(Action continuation) {
            this.Awaiter.OnCompleted(continuation);
        }

        public IEnumerableAwaiter<T> GetAwaiter() {
            return this;
        }

        public bool IsCompleted {
            get {
                return this.Awaiter.IsCompleted;
            }
        }

        public IEnumerable<T> GetResult() {
            return this.Awaiter.GetResult();
        }
    }
}