namespace Dashing.Weaver {
    using System;

    internal class WeaveException : Exception
    {
        public WeaveException(string message) : base(message)
        {
        }
    }
}