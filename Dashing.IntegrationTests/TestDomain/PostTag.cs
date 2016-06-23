namespace Dashing.IntegrationTests.TestDomain {
    public class PostTag {
        public long PostTagId { get; set; }

        public Post Post { get; set; }

        public Tag Tag { get; set; }
    }
}