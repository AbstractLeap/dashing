namespace Dashing.Weaver {
    using System;
    using System.Diagnostics;

    public class TimedOperation : IDisposable {
        private Stopwatch Stopwatch { get; set; }

        public TimedOperation(string description) {
            Console.Write(description);
            this.Stopwatch = new Stopwatch();
            this.Stopwatch.Start();
        }

        public TimedOperation(string format, params object[] args)
            : this(string.Format(format, args)) {
        }

        public void Dispose() {
            this.Stopwatch.Stop();
            Console.WriteLine(" ({0}ms)", this.Stopwatch.ElapsedMilliseconds);
        }
    }
}