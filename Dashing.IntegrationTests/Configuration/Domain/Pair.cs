namespace Dashing.IntegrationTests.Configuration.Domain {
    public class Pair {
        public virtual int PairId { get; set; }

        public virtual Pair References { get; set; }

        public virtual Pair ReferencedBy { get; set; }
    }
}