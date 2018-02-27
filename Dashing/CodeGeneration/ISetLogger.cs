namespace Dashing.CodeGeneration {
    using System.Collections.Generic;

    public interface ISetLogger {
        IEnumerable<string> GetSetProperties();

        bool IsSetLoggingEnabled();

        void EnableSetLogging();

        void DisableSetLogging();
    }
}