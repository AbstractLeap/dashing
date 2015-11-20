namespace PerformanceTest.Tests.EF {
    using System.Data.Entity;

    using PerformanceTest.Domain;

    internal class EfContext : DbContext {
        public EfContext()
            : base(Program.ConnectionString.ConnectionString) {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Post>().HasOptional(p => p.Author).WithMany().Map(e => e.MapKey("AuthorId"));
            modelBuilder.Entity<Post>().HasOptional(p => p.Blog).WithMany(b => b.Posts).Map(e => e.MapKey("BlogId"));
            modelBuilder.Entity<Comment>().HasOptional(c => c.Post).WithMany().Map(e => e.MapKey("PostId"));
            modelBuilder.Entity<Comment>().HasOptional(c => c.User).WithMany().Map(e => e.MapKey("UserId"));
            modelBuilder.Entity<Post>().HasMany(p => p.Comments).WithOptional(c => c.Post);
            modelBuilder.Entity<Post>().HasMany(p => p.Tags).WithOptional(p => p.Post);
            modelBuilder.Entity<PostTag>().HasOptional(c => c.Post).WithMany().Map(e => e.MapKey("PostId"));
            modelBuilder.Entity<PostTag>().HasOptional(c => c.Tag).WithMany().Map(e => e.MapKey("TagId"));
        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Comment> Comments { get; set; }
    }
}