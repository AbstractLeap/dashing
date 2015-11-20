namespace LightSpeed.Domain {
    using System.CodeDom.Compiler;
    using System.Linq;

    using Mindscape.LightSpeed;
    using Mindscape.LightSpeed.Linq;

    /// <summary>
    ///     Provides a strong-typed unit of work for working with the Test model.
    /// </summary>
    [GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
    public class TestUnitOfWork : UnitOfWork {
        public IQueryable<User> Users
        {
            get
            {
                return this.Query<User>();
            }
        }

        public IQueryable<Tag> Tags
        {
            get
            {
                return this.Query<Tag>();
            }
        }

        public IQueryable<PostTag> PostTags
        {
            get
            {
                return this.Query<PostTag>();
            }
        }

        public IQueryable<Blog> Blogs
        {
            get
            {
                return this.Query<Blog>();
            }
        }

        public IQueryable<Like> Likes
        {
            get
            {
                return this.Query<Like>();
            }
        }

        public IQueryable<Post> Posts
        {
            get
            {
                return this.Query<Post>();
            }
        }

        public IQueryable<Comment> Comments
        {
            get
            {
                return this.Query<Comment>();
            }
        }
    }
}