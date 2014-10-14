using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Dashing.CodeGeneration {
    internal class FetchCollectionAwaiter<T> : IDictionaryAwaiter<T> {
        public TaskAwaiter<IEnumerable<T>> Awaiter { get; set; }

        public IDictionary Results { get; set; }

        public IDictionaryAwaiter<T> GetAwaiter() {
            return this;
        }

        public bool IsCompleted {
            get { return this.Awaiter.IsCompleted; }
        }

        public IDictionary GetResult() {
            return this.Results;
        }

        public void OnCompleted(Action continuation) {
            this.Awaiter.OnCompleted(continuation);
        }
    }
}
