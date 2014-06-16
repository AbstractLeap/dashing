namespace PerformanceTest {
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;

    using Dapper;

    using TopHat;
    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Engine.DDL;

    internal class Program {
        public const string ConnectionString = "Data Source=.;Initial Catalog=tempdb;Integrated Security=True";

        private static void Main(string[] args) {
            var config = new TopHatConfiguration(new SqlServerEngine(), ConnectionString);
            var topHatWatch = new Stopwatch();
            var dapperWatch = new Stopwatch();

            using (var session = config.BeginSession()) {
                SetupDatabase(config, session);

                Iteration(session, 1);
                DapperIteration(session.Connection, 1);

                for (var j = 1; j <= 10; ++j) {
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
                }
            }

            Console.WriteLine("TopHat took {0}ms", topHatWatch.ElapsedMilliseconds);
            Console.WriteLine("Dapper took {0}ms", dapperWatch.ElapsedMilliseconds);
        }

        private static Post Iteration(ISession session, int i) {
            return session.Query<Post>().Where(p => p.PostId == i).First();
        }

        private static Post DapperIteration(IDbConnection connection, int i) {
            return connection.Query<Post>("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([PostId] = @l_1)", new { l_1 = i }).First();
        }

        private static void SetupDatabase(TopHatConfiguration config,  ISession session) {
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
    }
}