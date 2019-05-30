namespace Dashing.PerformanceTests.Domain {
    using System;

    public class Comment {
        public Comment() {
            this.CommentDate = DateTime.Now;
        }

        public virtual int CommentId { get; set; }

        public virtual string Content { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }

        public virtual DateTime CommentDate { get; set; }
    }
}