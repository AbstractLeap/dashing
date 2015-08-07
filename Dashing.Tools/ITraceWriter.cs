namespace Dashing.Tools {
    using System.Collections;
    using System.Collections.Generic;

    public interface ITraceWriter {
        void Trace(string message);

        void Trace(string message, params object[] args);

        void Trace<T>(IEnumerable<T> items, string[] columnHeaders = null);
    }
}