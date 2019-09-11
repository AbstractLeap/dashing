namespace Dashing.Weaver.Weaving.Weavers {
    using Dashing.Logging;

    public interface ITaskLogHelper {
        ILog Logger { get; set; }
    }
}