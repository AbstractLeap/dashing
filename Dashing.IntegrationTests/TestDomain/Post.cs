namespace Dashing.IntegrationTests.TestDomain {
    using System.Collections.Generic;

    public class Post {
        public Post() {
            this.Comments = new List<Comment>();
            this.Tags = new List<PostTag>();
        }

        public long PostId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public decimal Rating { get; set; }

        public User Author { get; set; }

        public Blog Blog { get; set; }

        public IList<Comment> Comments { get; set; }

        public IList<PostTag> Tags { get; set; }

        public bool DoNotMap { get; set; }
    }
}