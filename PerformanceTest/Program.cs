namespace PerformanceTest {
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using TopHat;
    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Engine.DDL;

    internal static class Program {
        private const string ConnectionString = "Data Source=.;Initial Catalog=tempdb;Integrated Security=True";

        private static void Main(string[] args) {
            var config = new TopHatConfiguration(new SqlServerEngine(), ConnectionString);
            var topHatWatch = new Stopwatch();
            var dapperWatch = new Stopwatch();
            var efWatch = new Stopwatch();
            var simpleDataWatch = new Stopwatch();

            var efDb = new EfContext();
            var simpleDataDb = Simple.Data.Database.OpenConnection(Program.ConnectionString);
            using (var session = config.BeginSession()) {
                SetupDatabase(config, session);

                Iteration(session, 1);
                DapperIteration(session.Connection, 1);
                EfIteration(efDb, 1);
                SimpleDataIteration(simpleDataDb, 1);

                for (var j = 1; j <= 3; ++j) {
                    for (var i = 1; i <= 500; i++) {
                        topHatWatch.Start();
                        Iteration(session, 1 + (i % 500));
                        topHatWatch.Stop();
                    }

                    for (var i = 1; i <= 500; i++) {
                        dapperWatch.Start();
                        DapperIteration(session.Connection, 1 + (i % 500));
                        dapperWatch.Stop();
                    }

                    for (var i = 1; i <= 500; i++) {
                        efWatch.Start();
                        EfIteration(efDb, i);
                        efWatch.Stop();
                    }

                    for (var i = 1; i <= 500; i++) {
                        simpleDataWatch.Start();
                        SimpleDataIteration(simpleDataDb, i);
                        simpleDataWatch.Stop();
                    }
                }
            }
            efDb.Dispose();

            Console.WriteLine("TopHat took {0}ms for 3 iterations of 500", topHatWatch.ElapsedMilliseconds);
            Console.WriteLine("Dapper took {0}ms for 3 iterations of 500", dapperWatch.ElapsedMilliseconds);
            Console.WriteLine("Entity Framework took {0}ms for 3 iterations of 500", efWatch.ElapsedMilliseconds);
            Console.WriteLine("Simple Data took {0}ms for 3 iterations of 500", simpleDataWatch.ElapsedMilliseconds);
        }

        private static Post SimpleDataIteration(dynamic db, int i) {
            return db.Posts.Get(i);
        }

        private static Post EfIteration(EfContext context, int i) {
            return context.Posts.AsNoTracking().First(p => p.PostId == i);
        }

        private static Post Iteration(ISession session, int i) {
            return session.Query<Post>().Where(p => p.PostId == i).First();
        }

        private static Post DapperIteration(IDbConnection connection, int i) {
            return
                connection.Query<Post>("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", new { l_1 = i })
                          .First();
        }

        private static void SetupDatabase(TopHatConfiguration config, ISession session) {
            var d = new SqlServerDialect();
            var dtw = new DropTableWriter(d);
            var ctw = new CreateTableWriter(d);
            var dropTables = config.Maps.Select(dtw.DropTableIfExists);
            var createTables = config.Maps.Select(ctw.CreateTable);
            var sqls = dropTables.Concat(createTables).ToArray();

            foreach (var sql in sqls) {
                session.Connection.Execute(sql);
            }

            var a = new User { UserId = 0 };
            var b = new Blog { BlogId = 0 };
            for (var i = 0; i <= 500; i++) {
                session.Insert(new Post { PostId = i, Author = a, Blog = b });
            }
        }

        private class TopHatConfiguration : DefaultConfiguration {
            public TopHatConfiguration(IEngine engine, string connectionString)
                : base(engine, connectionString) {
                this.Add<Blog>();
                this.Add<Comment>();
                this.Add<Post>();
                this.Add<User>();
            }
        }

        private class EfContext : DbContext {
            public EfContext() : base(Program.ConnectionString) { }

            protected override void OnModelCreating(DbModelBuilder modelBuilder) {
                modelBuilder.Entity<Post>().HasOptional(p => p.Author).WithMany().Map(e => e.MapKey("AuthorId"));
                modelBuilder.Entity<Post>().HasOptional(p => p.Blog).WithMany(b => b.Posts).Map(e => e.MapKey("BlogId"));
            }

            public DbSet<Post> Posts { get; set; }

            public DbSet<User> Users { get; set; }

            public DbSet<Blog> Blogs { get; set; }

            public DbSet<Comment> Comments { get; set; }
        }
    }
}