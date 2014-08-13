namespace Dashing.Tools.Tests.TestDomain {
    public class PostTag {
        public virtual int PostTagId { get; set; }

        public virtual Post Post { get; set; }

        public virtual Tag Tag { get; set; }
    }
}
