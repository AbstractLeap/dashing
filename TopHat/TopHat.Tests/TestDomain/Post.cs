using System.Collections.Generic;

namespace TopHat.Tests.TestDomain
{
    public class Post
    {
        public virtual int PostId { get; set; }

        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual decimal Rating { get; set; }

        public virtual User Author { get; set; }

        public virtual Blog Blog { get; set; }

        public virtual IList<Comment> Comments { get; set; }

        public virtual bool DoNotMap { get; set; }
    }
}