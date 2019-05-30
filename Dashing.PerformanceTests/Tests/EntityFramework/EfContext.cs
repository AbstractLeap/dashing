namespace Dashing.PerformanceTests.Tests.EntityFramework {
#if NET46
    using System.Data.Entity;

    using global::Dashing.PerformanceTests.Domain;

    using PerformanceTest;

        internal class EfContext : DbContext {
        public EfContext()
            : base(Program.ConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            // @formatter:off
            modelBuilder.Entity<Post>().HasOptional(p => p.Author).WithMany().Map(e => e.MapKey("AuthorId"));
            modelBuilder.Entity<Post>().HasOptional(p => p.Blog).WithMany(b => b.Posts).Map(e => e.MapKey("BlogId"));
            modelBuilder.Entity<Comment>().HasOptional(c => c.Post).WithMany().Map(e => e.MapKey("PostId"));
            modelBuilder.Entity<Comment>().HasOptional(c => c.User).WithMany().Map(e => e.MapKey("UserId"));
            modelBuilder.Entity<Post>().HasMany(p => p.Comments).WithOptional(c => c.Post);
            modelBuilder.Entity<Post>().HasMany(p => p.Tags).WithOptional(p => p.Post);
            modelBuilder.Entity<PostTag>().HasOptional(c => c.Post).WithMany().Map(e => e.MapKey("PostId"));
            modelBuilder.Entity<PostTag>().HasOptional(c => c.Tag).WithMany().Map(e => e.MapKey("TagId"));
            // @formatter:on
        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Comment> Comments { get; set; }
    }

#endif
    
}