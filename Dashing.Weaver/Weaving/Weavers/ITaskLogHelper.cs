namespace Dashing.Weaver.Weaving.Weavers {
    using Poly.Logging;

    public interface ITaskLogHelper {
        IPolyLogger PolyLogger { get; set; }
    }
}