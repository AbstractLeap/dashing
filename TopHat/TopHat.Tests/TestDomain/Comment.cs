using System;

namespace TopHat.Tests.TestDomain
{
    public class Comment
    {
        public virtual int CommentId { get; set; }

        public virtual string Content { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }

        public virtual DateTime CommentDate { get; set; }
    }
}