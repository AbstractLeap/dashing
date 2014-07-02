namespace PerformanceTest {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using ServiceStack.OrmLite;

    using Dashing;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.DDL;

    using Database = Simple.Data.Database;
    using SqlServerDialect = ServiceStack.OrmLite.SqlServerDialect;

    internal static class Program {
        private static ConnectionStringSettings ConnectionString = new ConnectionStringSettings("Default", "Data Source=.;Initial Catalog=tempdb;Integrated Security=True", "System.Data.SqlClient");

        private class Test {
            public Test(string provider, string name, Action<int> func) {
                this.Provider = provider;
                this.TestName = name;
                this.TestFunc = func;
            }

            public string Provider { get; set; }

            public string Method { get; set; }

            public string TestName { get; set; }

            public Action<int> TestFunc { get; set; }
        }

        private static void Main(string[] args) {
            var tests = new List<Test>();
            SetupTests(tests);

            var providers = tests.Select(t => t.Provider).Distinct().ToList();
            var testNames = tests.Select(t => t.TestName).Distinct().ToList();

            foreach (var name in testNames) {
                Console.WriteLine("Running " + name);
                Console.WriteLine("------------------------");
                foreach (var provider in providers) {
                    var providerTests = tests.Where(t => t.Provider == provider && t.TestName == name);
                    foreach (var test in providerTests) {
                        if (test != null) {
                            // warm up
                            test.TestFunc(1);
                            var watch = new Stopwatch();

                            // iterate 
                            for (int j = 0; j < 3; ++j) {
                                for (int i = 1; i <= 500; ++i) {
                                    watch.Start();
                                    test.TestFunc(i);
                                    watch.Stop();
                                }
                            }

                            Console.WriteLine(provider + (test.Method != null ? " (" + test.Method + ") " : string.Empty) + " : " + watch.ElapsedMilliseconds + "ms");
                        }
                    }
                }

                Console.WriteLine();
            }

            FinishTests();
        }

        private static void FinishTests() {
            EfDb.Dispose();
            ormliteConn.Dispose();
            session.Dispose();
        }

        private static readonly EfContext EfDb = new EfContext();

        private static IDbConnection ormliteConn;

        private static ISession session;

        private static void SetupTests(List<Test> tests) {
            var config = new DashingConfiguration(ConnectionString);
            var simpleDataDb = Database.OpenConnection(ConnectionString.ConnectionString);
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString.ConnectionString, SqlServerDialect.Provider);
            ormliteConn = dbFactory.OpenDbConnection();
            session = config.BeginSession();
            SetupDatabase(config, session);

            SetupSelectSingleTest(tests, simpleDataDb);
            SetupFetchTest(tests);
            SetupFetchChangeTests(tests);
        }

        private static void SetupFetchChangeTests(List<Test> tests) {
            var testName = "Get And Change";
            var r = new Random();

            // dapper
            tests.Add(
                new Test(
                    Providers.Dapper,
                    testName,
                    i => {
                        var post =
                            session.Connection.Query<Post>(
                                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                                new { l_1 = i }).First();
                        post.Title = Providers.Dapper + "_" + i + r.Next(100000);
                        session.Connection.Execute("Update [Posts] set [Title] = @Title where [PostId] = @PostId", new { post.Title, post.PostId });
                        var thatPost =
                            session.Connection.Query<Post>(
                                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                                new { l_1 = i }).First();
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(testName + " failed for " + Providers.Dapper + " as the update did not work");
                        }
                    }));

            // add Dashing
            tests.Add(
                new Test(
                    Providers.Dashing,
                    testName,
                    i => {
                        var post = session.Query<Post>().AsTracked().Where(p => p.PostId == i).First();
                        post.Title = Providers.Dashing + "_" + i + r.Next(100000);
                        session.Update(post);
                        var thatPost = session.Query<Post>().Where(p => p.PostId == i).First();
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(testName + " failed for " + Providers.Dashing + " as the update did not work");
                        }
                    }));

            // add Dashing by id method
            tests.Add(
                new Test(
                    Providers.Dashing,
                    testName,
                    i => {
                        var post = session.Get<Post>(i, true);
                        post.Title = Providers.Dashing + "_" + i + r.Next(100000);
                        session.Update(post);
                        var thatPost = session.Get<Post>(i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(testName + " failed for " + Providers.Dashing + " as the update did not work");
                        }
                    }) { Method = "By Id" });

            // add ef
            tests.Add(
                new Test(
                    Providers.EF,
                    testName,
                    i => {
                        var post = EfDb.Posts.Single(p => p.PostId == i);
                        post.Title = Providers.EF + "_" + i + r.Next(100000);
                        EfDb.SaveChanges();
                        var thatPost = EfDb.Posts.Single(p => p.PostId == i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(testName + " failed for " + Providers.EF + " as the update did not work");
                        }
                    }));

            // add servicestack
            tests.Add(
                new Test(
                    Providers.ServiceStack,
                    testName,
                    i => {
                        var post = ormliteConn.SingleById<Post>(i);
                        post.Title = Providers.ServiceStack + "_" + i + r.Next(100000);
                        ormliteConn.Update(post);
                        var thatPost = ormliteConn.SingleById<Post>(i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(testName + " failed for " + Providers.ServiceStack + " as the update did not work");
                        }
                    }));
        }

        private static void SetupFetchTest(List<Test> tests) {
            var testName = "Fetch";

            // add dapper
            tests.Add(
                new Test(
                    Providers.Dapper,
                    testName,
                    i =>
                    session.Connection.Query<Post, User, Post>(
                        "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where ([PostId] = @l_1)",
                        (p, u) => {
                            p.Author = u;
                            return p;
                        },
                        new { l_1 = i },
                        splitOn: "UserId").First()));

            // add Dashing
            tests.Add(new Test(Providers.Dashing, testName, i => session.Query<Post>().Fetch(p => p.Author).Where(p => p.PostId == i).First()));

            // add ef
            tests.Add(new Test(Providers.EF, testName, i => EfDb.Posts.AsNoTracking().Include(p => p.Author).Where(p => p.PostId == i).First()));
        }

        private static void SetupSelectSingleTest(List<Test> tests, dynamic simpleDataDb) {
            var testName = "SelectSingle";

            // add dapper
            tests.Add(
                new Test(
                    Providers.Dapper,
                    testName,
                    i =>
                    session.Connection.Query<Post>(
                        "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                        new { l_1 = i }).First()));

            // add Dashing
            tests.Add(new Test(Providers.Dashing, testName, i => session.Query<Post>().Where(p => p.PostId == i).First()));

            // add tophaqt by id
            tests.Add(new Test(Providers.Dashing, testName, i => session.Get<Post>(i)) { Method = "By Id" });

            // add ef
            tests.Add(new Test(Providers.EF, testName, i => EfDb.Posts.AsNoTracking().First(p => p.PostId == i)));

            // add ef2
            tests.Add(new Test(Providers.EF, testName, i => EfDb.Posts.Find(i)) { Method = "Using Find" });

            // add ormlite
            tests.Add(new Test(Providers.ServiceStack, testName, i => ormliteConn.SingleById<Post>(i)));

            // add simple data
            tests.Add(new Test(Providers.SimpleData, testName, i => simpleDataDb.Posts.Get(i)));
        }

        private static void SetupDatabase(DashingConfiguration config, ISession session) {
            var d = new Dashing.Engine.SqlServerDialect();
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

        private class DashingConfiguration : DefaultConfiguration {
            public DashingConfiguration(System.Configuration.ConnectionStringSettings connectionString)
                : base(connectionString) {
                this.Add<Blog>();
                this.Add<Comment>();
                this.Add<Post>();
                this.Add<User>();
            }
        }

        private class EfContext : DbContext {
            public EfContext()
                : base(Program.ConnectionString.ConnectionString) {
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder) {
                modelBuilder.Entity<Post>().HasOptional(p => p.Author).WithMany().Map(e => e.MapKey("AuthorId"));
                modelBuilder.Entity<Post>().HasOptional(p => p.Blog).WithMany(b => b.Posts).Map(e => e.MapKey("BlogId"));
            }

            public DbSet<Post> Posts { get; set; }

            public DbSet<User> Users { get; set; }

            public DbSet<Blog> Blogs { get; set; }

            public DbSet<Comment> Comments { get; set; }
        }

        private class Providers {
            public static string Dapper {
                get {
                    return "Dapper";
                }
            }

            public static string Dashing {
                get {
                    return "Dashing";
                }
            }

            public static string EF {
                get {
                    return "EF";
                }
            }

            public static string ServiceStack {
                get {
                    return "ServiceStack";
                }
            }

            public static string SimpleData {
                get {
                    return "Simple.Data";
                }
            }
        }
    }
}