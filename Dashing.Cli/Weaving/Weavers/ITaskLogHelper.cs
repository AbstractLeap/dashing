namespace Dashing.Cli.Weaving.Weavers {
    using Dashing.Tools;

    public interface ITaskLogHelper {
        ILogger Log { get; set; }
    }
}