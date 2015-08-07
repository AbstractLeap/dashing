namespace Dashing.Tools {
    using System.Collections.Generic;

    public interface ILogger {
        void Trace(string message);

        void Trace(string message, params object[] args);

        void Trace<T>(IEnumerable<T> items, string[] columnHeaders = null);

        void Error(string message);

        void Error(string message, params object[] args);
    }
}