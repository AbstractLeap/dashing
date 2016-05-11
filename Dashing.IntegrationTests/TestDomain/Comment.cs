namespace Dashing.IntegrationTests.TestDomain {
    using System;

    public class Comment {
        public Comment() {
            this.CommentDate = DateTime.Now;
        }

        public long CommentId { get; set; }

        public string Content { get; set; }

        public Post Post { get; set; }

        public User User { get; set; }

        public DateTime CommentDate { get; set; }
    }
}