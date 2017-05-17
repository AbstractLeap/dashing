namespace PerformanceTest.Domain {
    using System.Collections.Generic;

    using ServiceStack.DataAnnotations;

    [Alias("Posts")]
    public class Post {
        public Post() {
            this.Comments = new List<Comment>();
            this.Tags = new List<PostTag>();
        }

        [AutoIncrement]
        [PrimaryKey]
        public virtual int PostId { get; set; }

        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual decimal Rating { get; set; }

        [Ignore]
        public virtual User Author { get; set; }

        [Ignore]
        public virtual Blog Blog { get; set; }

        [Ignore]
        public virtual IList<Comment> Comments { get; set; }

        [Ignore]
        public virtual IList<PostTag> Tags { get; set; }

        public virtual bool DoNotMap { get; set; }
    }
}