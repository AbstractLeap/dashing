namespace Dashing.Tests.TestDomain {
    using System;
    using System.Collections.Generic;

    public class Comment {
        public Comment() {
            this.Likes = new List<Like>();
        }

        public virtual int CommentId { get; set; }

        public virtual string Content { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }

        public virtual DateTime CommentDate { get; set; }

        public virtual IList<Like> Likes { get; set; }
    }
}