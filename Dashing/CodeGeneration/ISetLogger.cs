namespace Dashing.CodeGeneration {
    using System.Collections.Generic;

    public interface ISetLogger {
        IEnumerable<string> GetSetProperties();
    }
}