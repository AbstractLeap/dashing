namespace Dashing.Console.Weaving.Weavers {
    using Dashing.Tools;

    using Microsoft.Build.Utilities;

    public interface ITaskLogHelper {
        ILogger Log { get; set; } 
    }
}