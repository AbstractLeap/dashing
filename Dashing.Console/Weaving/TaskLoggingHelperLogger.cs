namespace Dashing.Console.Weaving {
    using System;
    using System.Collections.Generic;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    using ILogger = Dashing.Tools.ILogger;

    public class TaskLoggingHelperLogger : ILogger {
        private readonly TaskLoggingHelper log;

        public TaskLoggingHelperLogger(TaskLoggingHelper log) {
            this.log = log;
        }

        public void Trace(string message) {
            this.log.LogMessage(MessageImportance.Normal, message);
        }

        public void Trace(string message, params object[] args) {
            this.log.LogMessage(MessageImportance.Normal, message, args);
        }

        public void Trace<T>(IEnumerable<T> items, string[] columnHeaders = null) {
            throw new NotImplementedException();
        }

        public void Error(string message) {
            this.log.LogError(message);
        }

        public void Error(string message, params object[] args) {
            this.log.LogError(message, args);
        }
    }
}