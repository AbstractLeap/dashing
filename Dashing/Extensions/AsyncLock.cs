namespace Dashing.Extensions {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// see http://www.hanselman.com/blog/ComparingTwoTechniquesInNETAsynchronousCoordinationPrimitives.aspx
    /// </summary>
    public sealed class AsyncLock {
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);

        private readonly Task<IDisposable> m_releaser;

        public AsyncLock() {
            this.m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync() {
            var wait = this.m_semaphore.WaitAsync();
            return wait.IsCompleted
                       ? this.m_releaser
                       : wait.ContinueWith(
                           (_, state) => (IDisposable)state,
                           this.m_releaser.Result,
                           CancellationToken.None,
                           TaskContinuationOptions.ExecuteSynchronously,
                           TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease) {
                this.m_toRelease = toRelease;
            }

            public void Dispose() {
                this.m_toRelease.m_semaphore.Release();
            }
        }
    }
}