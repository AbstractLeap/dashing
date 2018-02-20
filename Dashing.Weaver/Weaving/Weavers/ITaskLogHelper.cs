namespace Dashing.Weaver.Weaving.Weavers {
    using ILogger = Dashing.ILogger;

    public interface ITaskLogHelper {
        ILogger Log { get; set; }
    }
}