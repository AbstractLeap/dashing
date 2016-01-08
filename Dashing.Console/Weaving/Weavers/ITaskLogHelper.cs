namespace Dashing.Console.Weaving.Weavers {
    using Dashing.Tools;

    public interface ITaskLogHelper {
        ILogger Log { get; set; }
    }
}