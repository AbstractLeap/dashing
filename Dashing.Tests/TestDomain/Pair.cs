namespace Dashing.Tools.Tests.TestDomain {
    public class Pair {
        public virtual int PairId { get; set; }

        public virtual Pair References { get; set; }

        public virtual Pair ReferencedBy { get; set; }
    }
}