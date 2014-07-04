namespace PerformanceTest.Tests.EF {
    using System.Data.Entity;

    using PerformanceTest.Domain;

    internal class EfContext : DbContext {
        public EfContext()
            : base(Program.ConnectionString.ConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Post>()
                        .HasOptional(p => p.Author)
                        .WithMany()
                        .Map(e => e.MapKey("AuthorId"));
            modelBuilder.Entity<Post>()
                        .HasOptional(p => p.Blog)
                        .WithMany(b => b.Posts)
                        .Map(e => e.MapKey("BlogId"));
        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Comment> Comments { get; set; }
    }
}