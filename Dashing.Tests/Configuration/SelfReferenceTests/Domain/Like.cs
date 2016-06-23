namespace Dashing.Tests.Configuration.SelfReferenceTests.Domain {
    public class Like {
        public virtual int LikeId { get; set; }

        public virtual User User { get; set; }

        public virtual Comment Comment { get; set; }
    }
}