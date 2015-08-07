namespace Dashing.CodeGeneration.Weaving.Weavers {
    using Microsoft.Build.Utilities;

    public interface ITaskLogHelper {
        TaskLoggingHelper Log { get; set; } 
    }
}