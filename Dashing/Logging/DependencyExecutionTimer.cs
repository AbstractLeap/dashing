namespace Dashing.Logging
{
    using System;

    internal class DependencyExecutionTimer : IDisposable {
        private readonly Action<TimeSpan> continuation;

        private bool hasReturned;

        private DateTimeOffset startTime;

        internal DependencyExecutionTimer(Action<TimeSpan> continuation) {
            this.continuation = continuation;
            this.startTime = DateTime.UtcNow;
        }

        public void Stop() {
            this.CallContinuation();
        }

        public void Dispose() {
            this.CallContinuation();
        }

        private void CallContinuation() {

            if (this.hasReturned) {
                //We've already called the callback once, lets not call it again.
                return;
            }

            this.hasReturned = true;

            var duration = DateTime.UtcNow.Subtract(this.startTime.UtcDateTime);
            this.continuation?.Invoke(duration);
        }
    }
}
