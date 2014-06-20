namespace PerformanceTest {
    using System.Collections.Generic;

    using ServiceStack.DataAnnotations;
    using ServiceStack.OrmLite.SqlServer;

    [Alias("Posts")]
    public class Post {
        public virtual int PostId { get; set; }

        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual decimal Rating { get; set; }

        [Alias("AuthorId")]
        public virtual User Author { get; set; }

        [Alias("BlogId")]
        public virtual Blog Blog { get; set; }

        [Ignore]
        public virtual IList<Comment> Comments { get; set; }

        public virtual bool DoNotMap { get; set; }
    }
}