namespace PerformanceTest {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dapper;

    using Dashing;
    using Dashing.Configuration;
    using Dashing.Engine.DDL;

    using NHibernate;
    using NHibernate.Linq;

    using PerformanceTest.Domain;
    using PerformanceTest.Tests.Dashing;
    using PerformanceTest.Tests.EF;
    using PerformanceTest.Tests.NHibernate;

    using ServiceStack.OrmLite;

    using Simple.Data;

    using QueryableExtensions = System.Data.Entity.QueryableExtensions;

    internal static class Program {
        internal static readonly ConnectionStringSettings ConnectionString = new ConnectionStringSettings(
            "Default",
            "Data Source=.;Initial Catalog=tempdb;Integrated Security=True",
            "System.Data.SqlClient");

        private static void Main(string[] args) {
            SetupDatabase();

            var tests = new List<Test>();
            SetupTests(tests);

            var providers = tests.Select(t => t.Provider).Distinct().ToList();
            var testNames = tests.Select(t => t.TestName).Distinct().ToList();

            foreach (var name in testNames) {
                Console.WriteLine("Running " + name);
                Console.WriteLine("------------------------");
                var results = new List<System.Tuple<string, long>>();

                foreach (var provider in providers) {
                    var closeOverProvider = provider;
                    var closeOverName = name;

                    foreach (var test in tests.Where(t => t.Provider == closeOverProvider && t.TestName == closeOverName).Where(test => test != null)) {
                         {
                            // warm up
                            var watch = new Stopwatch();

                            // iterate 
                            for (var j = 0; j < 3; ++j) {
                                for (var i = 1; i <= 500; ++i) {
                                    watch.Start();
                                    test.TestFunc(i);
                                    watch.Stop();
                                }
                            }

                            results.Add(Tuple.Create(provider + (test.Method != null ? " (" + test.Method + ") " : string.Empty), watch.ElapsedMilliseconds));
                        }
                    }
                }

                foreach (var result in results.OrderBy(s => s.Item2)) {
                    Console.WriteLine("{0,7:N0} {1}", result.Item2, result.Item1);
                }

                Console.WriteLine();
            }
        }

        private static readonly IConfiguration dashingConfig = new DashingConfiguration(ConnectionString);

        private static readonly OrmLiteConnectionFactory connectionFactory = new OrmLiteConnectionFactory(ConnectionString.ConnectionString, SqlServerDialect.Provider);
        
        private static void SetupTests(List<Test> tests) {
            SetupSelectSingleTest(tests);
            SetupFetchTest(tests);
            SetupFetchChangeTests(tests);
            SetupFetchCollectionTests(tests);
            SetupFetchMultiCollectionTests(tests);
            SetupFetchMultipleMultiCollection(tests);
        }

        private static void SetupFetchMultipleMultiCollection(List<Test> tests) {
            const string TestName = "Fetch Multiple Multiple Collections";

            // add dashing
            tests.Add(
                new Test(
                    Providers.Dashing,
                    TestName,
                    i => {
                        using (var dashingSession = dashingConfig.BeginSession()) {
                            return
                                dashingSession.Query<Post>()
                                              .Fetch(p => p.Comments)
                                              .Fetch(p => p.Tags)
                                              .Where(p => p.PostId > i && p.PostId < i + 3)
                                              .ToList();

                        }
                    }));

            // add EF
            tests.Add(
                new Test(
                    Providers.EntityFramework,
                    TestName,
                    i => {
                        using (var EfDb = new EfContext()) {
                            return
                                QueryableExtensions.Include(
                                    QueryableExtensions.Include(EfDb.Posts, p => p.Tags),
                                    p => p.Comments)
                                                   .Where(p => p.PostId > i && p.PostId < i + 3)
                                                   .ToList();
                        }
                    }));

            // add nh stateful
            tests.Add(
                new Test(
                    Providers.NHibernate,
                    TestName,
                    i => {
                        using (var nhSession = Nh.SessionFactory.OpenSession()) {
                            // First(p => p.PostId == i) doesn't work?
                            // ok, nHIbernate linq broken (now I remember the pain!)
                            var posts =
                                nhSession.QueryOver<Post>()
                                         .Where(p => p.PostId > i && p.PostId < i + 3)
                                         .Future<Post>();
                            var comments =
                                nhSession.QueryOver<Post>()
                                         .Fetch(p => p.Comments)
                                         .Eager.Where(p => p.PostId > i && p.PostId < i + 3)
                                         .Future<Post>();
                            var tags =
                                nhSession.QueryOver<Post>()
                                         .Fetch(p => p.Tags)
                                         .Eager.Where(p => p.PostId > i && p.PostId < i + 3)
                                         .Future<Post>();
                            return posts.ToList();
                        }

                    },
                    "Stateful"));
        }

        private static void SetupFetchMultiCollectionTests(List<Test> tests) {
            const string TestName = "Fetch Multiple Collections";

            // add dashing
            tests.Add(
                new Test(
                    Providers.Dashing,
                    TestName,
                    i => {
                        using (var dashingSession = dashingConfig.BeginSession()) {
                            return
                                dashingSession.Query<Post>()
                                              .Fetch(p => p.Comments)
                                              .Fetch(p => p.Tags)
                                              .First(p => p.PostId == i);
                        }
                    }));

            // add EF
            tests.Add(
                new Test(
                    Providers.EntityFramework,
                    TestName,
                    i => {
                        using (var EfDb = new EfContext()) {
                            return
                                QueryableExtensions.Include(
                                    QueryableExtensions.Include(EfDb.Posts, p => p.Tags),
                                    p => p.Comments).First(p => p.PostId == i);
                        }
                    }));

            // add nh stateful
            tests.Add(
                new Test(
                    Providers.NHibernate,
                    TestName,
                    i => {
                        using (var nhSession = Nh.SessionFactory.OpenSession()) {
                            // First(p => p.PostId == i) doesn't work?
                            // ok, nHIbernate linq broken (now I remember the pain!)
                            var posts =
                                nhSession.QueryOver<Post>().Where(p => p.PostId == i).Future<Post>();
                            var comments =
                                nhSession.QueryOver<Post>()
                                         .Fetch(p => p.Comments)
                                         .Eager.Where(p => p.PostId == i)
                                         .Future<Post>();
                            var tags =
                                nhSession.QueryOver<Post>()
                                         .Fetch(p => p.Tags)
                                         .Eager.Where(p => p.PostId == i)
                                         .Future<Post>();
                            return posts.First();
                        }
                    },
                    "Stateful"));

            // add nh stateless
            // No can do, get NotSupportedException on first line here.
            //tests.Add(
            //    new Test(
            //        Providers.NHibernate,
            //        TestName,
            //        i => {
            //            // First(p => p.PostId == i) doesn't work?
            //            // ok, nHIbernate linq broken (now I remember the pain!)
            //            var posts = nhStatelessSession.QueryOver<Post>().Future<Post>();
            //            var comments =
            //                nhStatelessSession.QueryOver<Post>().Fetch(p => p.Comments).Eager.Future<Post>();
            //            var tags =
            //                nhStatelessSession.QueryOver<Post>().Fetch(p => p.Tags).Eager.Future<Post>();
            //            var post = posts.Where(p => p.PostId == i).First();
            //        },
            //        "Stateless"));
        }

        private static void SetupFetchCollectionTests(List<Test> tests) {
            const string TestName = "Fetch Collection";

            // add dapper
            tests.Add(
                new Test(
                    Providers.Dapper,
                    TestName,
                    i => {
                        using (var dapperConn = connectionFactory.OpenDbConnection()) {
                            var post =
                                dapperConn.Query<Post>(
                                    "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                                    new { l_1 = i }).First();
                            var comments =
                                dapperConn.Query<Comment>(
                                    "select * from [Comments] where [PostId] = @postId",
                                    new { postId = post.PostId }).ToList();
                            post.Comments = comments;
                            return post;
                        }
                    },
                    "Naive"));

            tests.Add(
                new Test(
                    Providers.Dapper,
                    TestName,
                    i => {
                        using (var dapperConn = connectionFactory.OpenDbConnection()) {
                            var sql = @"
select * from Posts where PostId = @id
select * from Comments where PostId = @id";

                            var multi = dapperConn.QueryMultiple(sql, new { id = i });
                            var post = multi.Read<Post>().Single();
                            post.Comments = multi.Read<Comment>().ToList();
                            multi.Dispose();
                            return post;
                        }
                    },
                    "Multiple Result Method"));

            // add Dashing
            tests.Add(
                new Test(
                    Providers.Dashing,
                    TestName,
                    i => {
                        using (var dashingSession = dashingConfig.BeginSession()) {
                            return
                                dashingSession.Query<Post>()
                                              .Fetch(p => p.Comments)
                                              .First(p => p.PostId == i);
                        }
                    }));

            // add EF
            tests.Add(new Test(Providers.EntityFramework, TestName,
                i => {
                    using (var EfDb = new EfContext()) {
                      return
                            QueryableExtensions.Include(EfDb.Posts, p => p.Comments)
                                               .First(p => p.PostId == i);
                    }
                }));

            // add nh stateful
            tests.Add(
                new Test(
                    Providers.NHibernate,
                    TestName,
                    i => {
                        using (var nhSession = Nh.SessionFactory.OpenSession()) {
                            return nhSession.Query<Post>().Fetch(p => p.Comments).First(p => p.PostId == i);
                        }
                    },
                    "Stateful"));

            // add nh stateless
            tests.Add(
                new Test(
                    Providers.NHibernate,
                    TestName,
                    i => {
                        using (var nhStatelessSession = Nh.SessionFactory.OpenStatelessSession()) {
                            return
                                nhStatelessSession.Query<Post>()
                                                  .Fetch(p => p.Comments)
                                                  .First(p => p.PostId == i);
                        }
                    },
                    "Stateless"));
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
                        using (var dapperConn = connectionFactory.OpenDbConnection()) {
                            var post =
                                dapperConn.Query<Post>(
                                    "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                                    new { l_1 = i }).First();
                            post.Title = Providers.Dapper + "_" + i + r.Next(100000);
                            dapperConn.Execute(
                                "Update [Posts] set [Title] = @Title where [PostId] = @PostId",
                                new { post.Title, post.PostId });
                            var thatPost =
                                dapperConn.Query<Post>(
                                    "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                                    new { l_1 = i }).First();
                            if (thatPost.Title != post.Title) {
                                Console.WriteLine(
                                    TestName + " failed for " + Providers.Dapper
                                    + " as the update did not work");
                            }

                            return post;
                        }
                    }));

            // add Dashing
            tests.Add(
                new Test(
                    Providers.Dashing,
                    TestName,
                    i => {
                        using (var dashingSession = dashingConfig.BeginSession()) {
                            var post =
                                dashingSession.Query<Post>().AsTracked().First(p => p.PostId == i);
                            post.Title = Providers.Dashing + "_" + i + r.Next(100000);
                            dashingSession.Save(post);
                            var thatPost = dashingSession.Query<Post>().First(p => p.PostId == i);
                            if (thatPost.Title != post.Title) {
                                Console.WriteLine(
                                    TestName + " failed for " + Providers.Dashing
                                    + " as the update did not work");
                            }

                            return post;
                        }
                    }));

            // add Dashing by id method
            tests.Add(
                new Test(
                    Providers.Dashing,
                    TestName,
                    i => {
                        using (var dashingSession = dashingConfig.BeginSession()) {
                            var post = dashingSession.GetTracked<Post>(i);
                            post.Title = Providers.Dashing + "_" + i + r.Next(100000);
                            dashingSession.Save(post);
                            var thatPost = dashingSession.Get<Post>(i);
                            if (thatPost.Title != post.Title) {
                                Console.WriteLine(
                                    TestName + " failed for " + Providers.Dashing
                                    + " as the update did not work");
                            }

                            return post;
                        }
                    },
                    "By Id"));

            // add ef
            tests.Add(
                new Test(
                    Providers.EntityFramework,
                    TestName,
                    i => {
                        using (var EfDb = new EfContext()) {
                            var post = EfDb.Posts.Single(p => p.PostId == i);
                            post.Title = Providers.EntityFramework + "_" + i + r.Next(100000);
                            EfDb.SaveChanges();
                            var thatPost = EfDb.Posts.Single(p => p.PostId == i);
                            if (thatPost.Title != post.Title) {
                                Console.WriteLine(
                                    TestName + " failed for " + Providers.EntityFramework
                                    + " as the update did not work");
                            }

                            return post;
                        }
                    }));

            // add servicestack
            tests.Add(
                new Test(
                    Providers.ServiceStack,
                    TestName,
                    i => {
                        using (var ormliteConn = connectionFactory.OpenDbConnection()) {
                            var post = ormliteConn.SingleById<Post>(i);
                            post.Title = Providers.ServiceStack + "_" + i + r.Next(100000);
                            ormliteConn.Update(post);
                            var thatPost = ormliteConn.SingleById<Post>(i);
                            if (thatPost.Title != post.Title) {
                                Console.WriteLine(
                                    TestName + " failed for " + Providers.ServiceStack
                                    + " as the update did not work");
                            }

                            return post;
                        }
                    }));

            // add nhibernate
            tests.Add(
                new Test(
                    Providers.NHibernate,
                    TestName,
                    i => {
                        using (var nhSession = Nh.SessionFactory.OpenSession()) {
                            var post = nhSession.Get<Post>(i);
                            post.Title = Providers.NHibernate + "_" + i + r.Next(100000);
                            nhSession.Update(post);
                            nhSession.Flush();
                            var thatPost = nhSession.Get<Post>(i);
                            if (thatPost.Title != post.Title) {
                                Console.WriteLine(
                                    TestName + " failed for " + Providers.NHibernate
                                    + " as the update did not work");
                            }

                            return post;
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
                    i => {
                        using (var dapperConn = connectionFactory.OpenDbConnection()) {
                            return dapperConn.Query<Post, User, Post>(
                                "select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[BlogId], t.[DoNotMap], t_1.[UserId], t_1.[Username], t_1.[EmailAddress], t_1.[Password], t_1.[IsEnabled], t_1.[HeightInMeters] from [Posts] as t left join [Users] as t_1 on t.AuthorId = t_1.UserId where ([PostId] = @l_1)",
                                (p, u) => {
                                    p.Author = u;
                                    return p;
                                },
                                new { l_1 = i },
                                splitOn: "UserId").First();
                        }
                    }));

            // add Dashing
            tests.Add(new Test(Providers.Dashing, TestName,
                i => {
                    using (var dashingSession = dashingConfig.BeginSession()) {
                        return dashingSession.Query<Post>().Fetch(p => p.Author).First(p => p.PostId == i);
                    }
                }));

            // add ef
            tests.Add(new Test(Providers.EntityFramework, TestName,
                i => {
                    using (var EfDb = new EfContext()) {
                        return
                            QueryableExtensions.Include(EfDb.Posts.AsNoTracking(), p => p.Author)
                                               .First(p => p.PostId == i);
                    }
                }));

            // add nh stateful
            tests.Add(
                new Test(Providers.NHibernate, TestName,
                    i => {
                        using (var nhSession = Nh.SessionFactory.OpenSession()) {
                            return nhSession.Query<Post>().Fetch(p => p.Author)
                                               .First(p => p.PostId == i);
                        }
                    }, "Stateful"));

            // add nh stateless
            tests.Add(
                new Test(
                    Providers.NHibernate,
                    TestName,
                    i => {
                        using (var nhStatelessSession = Nh.SessionFactory.OpenStatelessSession()) {
                            return
                                nhStatelessSession.Query<Post>().Fetch(p => p.Author)
                                               .First(p => p.PostId == i); ;
                        }
                    },
                    "Stateless"));
        }

        private static void SetupSelectSingleTest(List<Test> tests) {
            const string TestName = "SelectSingle";

            // add dapper
            tests.Add(
                new Test(
                    Providers.Dapper,
                    TestName,
                    i => {
                        using (var dapperConn = connectionFactory.OpenDbConnection()) {
                            return dapperConn.Query<Post>(
                                "select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)",
                                new { l_1 = i }).First();
                        }
                    }));

            // add Dashing
            tests.Add(new Test(Providers.Dashing, TestName,
                i => {
                    using (var dashingSession = dashingConfig.BeginSession()) {
                        return dashingSession.Query<Post>().First(p => p.PostId == i);
                    }
                }));

            // add Dashing by id
            tests.Add(new Test(Providers.Dashing, TestName,
                i => {
                    using (var dashingSession = dashingConfig.BeginSession()) {
                        return dashingSession.Get<Post>(i);
                    }
                }, "By Id"));

            // add ef
            tests.Add(new Test(Providers.EntityFramework, TestName,
                i => {
                    using (var EfDb = new EfContext()) {
                        return EfDb.Posts.AsNoTracking().First(p => p.PostId == i);
                    }
                }));

            // add ef2
            tests.Add(
                new Test(
                    Providers.EntityFramework,
                    TestName,
                    i => {
                        using (var EfDb = new EfContext()) {
                            EfDb.Configuration.AutoDetectChangesEnabled = false;
                            var post = EfDb.Posts.Find(i);
                            EfDb.Configuration.AutoDetectChangesEnabled = true;
                            return post;
                        }
                    },
                    "Using Find with AutoDetechChangesEnabled = false"));

            // add ormlite
            tests.Add(new Test(Providers.ServiceStack, TestName,
                i => {
                    using (var ormliteConn = connectionFactory.OpenDbConnection()) {
                        return ormliteConn.SingleById<Post>(i);
                    }
                }));

            // add simple data
            tests.Add(new Test(Providers.SimpleData, TestName,
                i => {
                    var simpleDataDb = Database.OpenConnection(ConnectionString.ConnectionString);
                        return simpleDataDb.Posts.Get(i);
                }));

            // add nh stateless
            tests.Add(new Test(Providers.NHibernate, TestName,
                i => {
                    using (var nhStatelessSession = Nh.SessionFactory.OpenStatelessSession()) {
                        return nhStatelessSession.Get<Post>(i);
                    }
                }, "Stateless"));

            // add nh stateful
            tests.Add(new Test(Providers.NHibernate, TestName,
                i => {
                    using (var nhSession = Nh.SessionFactory.OpenSession()) {
                        return nhSession.Get<Post>(i);
                    }
                }, "Stateful"));
        }

        private static void SetupDatabase() {
            var d = new Dashing.Engine.Dialects.SqlServerDialect();
            var dtw = new DropTableWriter(d);
            var ctw = new CreateTableWriter(d);
            var dropTables = dashingConfig.Maps.Select(dtw.DropTableIfExists);
            var createTables = dashingConfig.Maps.Select(ctw.CreateTable);
            var sqls = dropTables.Concat(createTables).ToArray();

            using (var setupSession = dashingConfig.BeginSession()) {
                foreach (var sql in sqls) {
                    setupSession.Connection.Execute(sql);
                }

                var r = new Random();
                var users = new List<User>();
                for (var i = 0; i < 100; i++) {
                    var user = new User();
                    users.Add(user);
                    setupSession.Insert(user);
                }

                var blogs = new List<Blog>();
                for (var i = 0; i < 100; i++) {
                    var blog = new Blog();
                    blogs.Add(blog);
                    setupSession.Insert(blog);
                }

                var posts = new List<Post>();
                for (var i = 0; i <= 500; i++) {
                    var userId = r.Next(100);
                    var blogId = r.Next(100);
                    var post = new Post { Author = users[userId], Blog = blogs[blogId] };
                    setupSession.Insert(post);
                    posts.Add(post);
                }

                for (var i = 0; i < 5000; i++) {
                    var comment = new Comment { Post = posts[r.Next(500)], User = users[r.Next(100)] };
                    setupSession.Insert(comment);
                }

                var tags = new List<Tag>();
                for (var i = 0; i < 100; i++) {
                    var tag = new Tag { Content = "Tag" + i };
                    tags.Add(tag);
                    setupSession.Insert(tag);
                }

                for (var i = 0; i < 5000; i++) {
                    var postTag = new PostTag { Post = posts[r.Next(500)], Tag = tags[r.Next(100)] };
                    setupSession.Insert(postTag);
                }


                setupSession.Complete();
            }
        }
    }
}