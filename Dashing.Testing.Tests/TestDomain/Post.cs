namespace Dashing.Testing.Tests.TestDomain {
    using System;
    using System.Collections.Generic;

    public class Post {
        public Post() {
            this.Comments = new List<Comment>();
            this.Tags = new List<PostTag>();
        }

        public virtual int PostId { get; set; }

        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual decimal Rating { get; set; }

        public virtual User Author { get; set; }

        public virtual Blog Blog { get; set; }

        public virtual IList<Comment> Comments { get; set; }

        public virtual IList<PostTag> Tags { get; set; }

        public virtual IList<PostTag> DeletedTags { get; set; }

        public virtual IList<PostTag> YetMoreTags { get; set; }

        public virtual bool DoNotMap { get; set; }

        public DateTime? DeletedDate { get; set; }
    }
}