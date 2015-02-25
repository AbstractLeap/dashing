namespace Dashing.Tests {
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.Engine.Dialects;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class FuckedOffTests {
        private readonly Configuration config;

        public FuckedOffTests() {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultDb"];

            this.config = new Configuration(connectionString);

            using (var session = this.config.BeginTransactionLessSession()) {
                var dialect = new DialectFactory().Create(connectionString);
                var ctw = new CreateTableWriter(dialect);
                var dtw = new DropTableWriter(dialect);

                var maps = this.config.Maps.OrderTopologically();
                foreach (var map in maps.OrderedMaps) {
                    session.Dapper.Execute(dtw.DropTableIfExists(map));
                }

                foreach (var map in maps.OrderedMaps.Reverse()) {
                    session.Dapper.Execute(ctw.CreateTable(map));
                }

                var james = new User { Username = "james" };
                var mark = new User { Username = "mark" };
                var blog = new Blog { Title = "blog of greatness", CreateDate = DateTime.UtcNow };
                var post = new Post { Title = "post of much postiness", Author = james, Blog = blog };
                var comment1 = new Comment { User = mark, Post = post, Content = "this is a bit shit", CommentDate = DateTime.UtcNow };
                session.Insert(james, mark);
                session.Insert(blog);
                session.Insert(post);
                session.Insert(comment1);
            }
        }

        [Fact]
        public async Task FetchOneAndACollectionReturnsOnlyOneRow() {
            var dashing = this.config.BeginSession();
            var query = dashing.Query<Post>();
            query.Fetch(p => p.Blog);
            query.FetchMany(p => p.Comments);

            var results = await query.ToListAsync();
            var post = results.SingleOrDefault();

            Assert.NotNull(post);
            Assert.NotNull(post.Author);
            Assert.NotNull(post.Blog);
            Assert.NotNull(post.Comments);
            Assert.Equal(1, post.Comments.Count);
        }

        public class Configuration : DefaultConfiguration {
            public Configuration(ConnectionStringSettings connectionString)
                : base(connectionString, new CodeGeneratorConfig { MapperGenerationMaxRecursion = 0, CompileInDebug = true }) {
                this.AddNamespaceOf<Blog>();
            }
        }
    }
}