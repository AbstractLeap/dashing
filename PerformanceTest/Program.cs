namespace PerformanceTest {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using Dashing;
    using Dashing.Configuration;
    using Dashing.Engine.DDL;

    using PerformanceTest.Domain;
    using PerformanceTest.Tests.Dashing;
    using PerformanceTest.Tests.EF;

    using ServiceStack.OrmLite;

    using Database = Simple.Data.Database;
    using SqlServerDialect = Dashing.Engine.Dialects.SqlServerDialect;

    internal static class Program {
        internal static readonly ConnectionStringSettings ConnectionString = new ConnectionStringSettings(
            "Default", 
            "Data Source=.;Initial Catalog=tempdb;Integrated Security=True", 
            "System.Data.SqlClient");

        private static void Main(string[] args) {
            var tests = new List<Test>();
            SetupTests(tests);

            var providers = tests.Select(t => t.Provider)
                                 .Distinct()
                                 .ToList();
            var testNames = tests.Select(t => t.TestName)
                                 .Distinct()
                                 .ToList();

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
                            for (var j = 0; j < 3; ++j) {
                                for (var i = 1; i <= 500; ++i) {
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
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString.ConnectionString, ServiceStack.OrmLite.SqlServerDialect.Provider);
            ormliteConn = dbFactory.OpenDbConnection();
            session = config.BeginSession();
            SetupDatabase(config);

            SetupSelectSingleTest(tests, simpleDataDb);
            SetupFetchTest(tests);
            SetupFetchChangeTests(tests);
        }

        private static void SetupFetchChangeTests(List<Test> tests) {
            const string TestName = "Get And Change";
            var r = new Random();

            // dapper
            tests.Add(
                new Test(
                    Providers.Dapper, 
                    TestName, 
                    i => {
                        var post =
                            session.Connection.Query<Post>(
                                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", 
                                new { l_1 = i })
                                   .First();
                        post.Title = Providers.Dapper + "_" + i + r.Next(100000);
                        session.Connection.Execute("Update [Posts] set [Title] = @Title where [PostId] = @PostId", new { post.Title, post.PostId });
                        var thatPost =
                            session.Connection.Query<Post>(
                                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", 
                                new { l_1 = i })
                                   .First();
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(TestName + " failed for " + Providers.Dapper + " as the update did not work");
                        }
                    }));

            // add Dashing
            tests.Add(
                new Test(
                    Providers.Dashing, 
                    TestName, 
                    i => {
                        var post = session.Query<Post>()
                                          .AsTracked()
                                          .First(p => p.PostId == i);
                        post.Title = Providers.Dashing + "_" + i + r.Next(100000);
                        session.Update(post);
                        var thatPost = session.Query<Post>()
                                              .First(p => p.PostId == i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(TestName + " failed for " + Providers.Dashing + " as the update did not work");
                        }
                    }));

            // add Dashing by id method
            tests.Add(
                new Test(
                    Providers.Dashing, 
                    TestName, 
                    i => {
                        var post = session.Get<Post>(i, true);
                        post.Title = Providers.Dashing + "_" + i + r.Next(100000);
                        session.Update(post);
                        var thatPost = session.Get<Post>(i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(TestName + " failed for " + Providers.Dashing + " as the update did not work");
                        }
                    }, 
                    "By Id"));

            // add ef
            tests.Add(
                new Test(
                    Providers.EntityFramework, 
                    TestName, 
                    i => {
                        var post = EfDb.Posts.Single(p => p.PostId == i);
                        post.Title = Providers.EntityFramework + "_" + i + r.Next(100000);
                        EfDb.SaveChanges();
                        var thatPost = EfDb.Posts.Single(p => p.PostId == i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(TestName + " failed for " + Providers.EntityFramework + " as the update did not work");
                        }
                    }));

            // add servicestack
            tests.Add(
                new Test(
                    Providers.ServiceStack, 
                    TestName, 
                    i => {
                        var post = ormliteConn.SingleById<Post>(i);
                        post.Title = Providers.ServiceStack + "_" + i + r.Next(100000);
                        ormliteConn.Update(post);
                        var thatPost = ormliteConn.SingleById<Post>(i);
                        if (thatPost.Title != post.Title) {
                            Console.WriteLine(TestName + " failed for " + Providers.ServiceStack + " as the update did not work");
                        }
                    }));
        }

        private static void SetupFetchTest(List<Test> tests) {
            const string TestName = "Fetch";

            // add dapper
            tests.Add(
                new Test(
                    Providers.Dapper, 
                    TestName, 
                    i =>
                    session.Connection.Query<Post, User, Post>(
                        "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where ([PostId] = @l_1)", 
                        (p, u) => {
                            p.Author = u;
                            return p;
                        }, 
                        new { l_1 = i }, 
                        splitOn: "UserId")
                           .First()));

            // add Dashing
            tests.Add(
                new Test(
                    Providers.Dashing, 
                    TestName, 
                    i => session.Query<Post>()
                                .Fetch(p => p.Author)
                                .First(p => p.PostId == i)));

            // add ef
            tests.Add(
                new Test(
                    Providers.EntityFramework, 
                    TestName, 
                    i => EfDb.Posts.AsNoTracking()
                             .Include(p => p.Author)
                             .First(p => p.PostId == i)));
        }

        private static void SetupSelectSingleTest(List<Test> tests, dynamic simpleDataDb) {
            const string TestName = "SelectSingle";

            // add dapper
            tests.Add(
                new Test(
                    Providers.Dapper, 
                    TestName, 
                    i =>
                    session.Connection.Query<Post>(
                        "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", 
                        new { l_1 = i })
                           .First()));

            // add Dashing
            tests.Add(
                new Test(
                    Providers.Dashing, 
                    TestName, 
                    i => session.Query<Post>().First(p => p.PostId == i)));

            // add Dashing by id
            tests.Add(new Test(Providers.Dashing, TestName, i => session.Get<Post>(i), "By Id"));

            // add ef
            tests.Add(
                new Test(
                    Providers.EntityFramework, 
                    TestName, 
                    i => EfDb.Posts.AsNoTracking()
                             .First(p => p.PostId == i)));

            // add ef2
            tests.Add(new Test(Providers.EntityFramework, TestName, i => EfDb.Posts.Find(i), "Using Find"));

            // add ormlite
            tests.Add(new Test(Providers.ServiceStack, TestName, i => ormliteConn.SingleById<Post>(i)));

            // add simple data
            tests.Add(new Test(Providers.SimpleData, TestName, i => simpleDataDb.Posts.Get(i)));
        }

        private static void SetupDatabase(IConfiguration dashingConfig) {
            var d = new SqlServerDialect();
            var dtw = new DropTableWriter(d);
            var ctw = new CreateTableWriter(d);
            var dropTables = dashingConfig.Maps.Select(dtw.DropTableIfExists);
            var createTables = dashingConfig.Maps.Select(ctw.CreateTable);
            var sqls = dropTables.Concat(createTables)
                                 .ToArray();

            foreach (var sql in sqls) {
                session.Connection.Execute(sql);
            }

            var a = new User { UserId = 0 };
            var b = new Blog { BlogId = 0 };
            for (var i = 0; i <= 500; i++) {
                session.Insert(new Post { PostId = i, Author = a, Blog = b });
            }
        }
    }
}