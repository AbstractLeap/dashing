namespace Dashing.Console {
    using System;

    internal class CatchyException : Exception {
        public CatchyException(string message)
            : base(message) { }

        public CatchyException(string format, params object[] args)
            : base(string.Format(format, args)) { }
    }
}