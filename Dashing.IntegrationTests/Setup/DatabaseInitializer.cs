namespace Dashing.IntegrationTests.Setup {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.IntegrationTests.TestDomain.More;
    using Dashing.Migration;
    using Moq;

    public class DatabaseInitializer {
        private readonly SqlDatabase sessionCreator;

        private readonly IConfiguration config;

        public const string DatabaseName = "dashingintegrationtests";

        public DatabaseInitializer(SqlDatabase sessionCreator, IConfiguration config) {
            this.sessionCreator = sessionCreator;
            this.config = config;
        }

        public TestSessionWrapper Initialize() {
            // load the data
            using (var transactionLessSession = this.sessionCreator.BeginTransactionLessSession()) {
                // create database if exists
                if (!this.sessionCreator.GetDialect().IgnoreMultipleDatabases) {
                    if (transactionLessSession.Dapper.Query(this.sessionCreator.GetDialect().CheckDatabaseExists(DatabaseName)).Any()) {
                        transactionLessSession.Dapper.Execute("drop database " + DatabaseName);
                    }

                    transactionLessSession.Dapper.Execute("create database " + DatabaseName);
                    transactionLessSession.Dapper.Execute("use " + DatabaseName);
                }
                
                var migrator = new Migrator(
                    this.sessionCreator.GetDialect(),
                    new CreateTableWriter(this.sessionCreator.GetDialect()),
                    new AlterTableWriter(this.sessionCreator.GetDialect()),
                    new DropTableWriter(this.sessionCreator.GetDialect()),
                    new StatisticsProvider(null, this.sessionCreator.GetDialect()));
                IEnumerable<string> warnings, errors;
                var createStatement = migrator.GenerateSqlDiff(
                    new List<IMap>(),
                    this.config.Maps,
                    null,
                    new string[0],
                    new string[0],
                    out warnings,
                    out errors);
                var statements = createStatement.Split(';');
                foreach (var statement in statements.Where(s => !string.IsNullOrWhiteSpace(s.Trim()))) {
                    transactionLessSession.Dapper.Execute(statement);
                }
            }

            var session = this.sessionCreator.BeginSession();
            if (!this.sessionCreator.GetDialect().IgnoreMultipleDatabases) {
                session.Dapper.Execute("use " + DatabaseName);
            }

            this.InsertData(session);
            return new TestSessionWrapper(session);
        }

        private void InsertData(ISession session) {
            var users = new List<User>();
            for (var i = 0; i < 10; i++) {
                var user = new User { Username = "User_" + i };
                users.Add(user);
                session.Insert(user);
            }

            var blogs = new List<Blog>();
            for (var i = 0; i < 10; i++) {
                var blog = new Blog { Title = "Blog_" + i };
                blogs.Add(blog);
                session.Insert(blog);
            }

            var posts = new List<Post>();
            for (var i = 0; i < 20; i++) {
                var userId = i / 2;
                var blogId = i / 2;
                var post = new Post { Author = users[userId], Blog = blogs[blogId], Title = "Post_" + i };
                session.Insert(post);
                posts.Add(post);
            }

            for (var i = 0; i < 30; i++) {
                var comment = new Comment { Post = posts[i / 2], User = users[i / 3], Content = "Comment_" + i };
                session.Insert(comment);
            }

            var tags = new List<Tag>();
            for (var i = 0; i < 20; i++) {
                var tag = new Tag { Content = "Tag_" + i };
                tags.Add(tag);
                session.Insert(tag);
            }

            for (var i = 0; i < 30; i++) {
                var postTag = new PostTag { Post = posts[i / 2], Tag = tags[i / 2] };
                session.Insert(postTag);
            }

            // insert single comment with null User to check that nulls are returned properly
            var nullUserComment = new Comment { Post = posts[0], User = null, Content = "Nullable User Content" };
            session.Insert(nullUserComment);

            var nullTag = new Tag { Content = "Null Post Tag" };
            session.Insert(nullTag);
            var nullPostTag = new PostTag { Tag = nullTag };
            session.Insert(nullPostTag);

            // add user for bulk update
            session.Insert(new User { Username = "BulkUpdate", Password = "Blah" });

            // add users for bulk delete
            session.Insert(new User { Username = "BulkDelete", Password = "Foo" }, new User { Username = "BulkDelete", Password = "Bar" });

            // test delete user
            session.Insert(new User { Username = "TestDelete", Password = "Blah" });

            // test empty collection
            session.Insert(new Blog { Title = "EmptyBlog" });

            // multiple fetch many stuff
            session.Insert(new Questionnaire { Name = "Foo" });
            session.Insert(new Question { Questionnaire = new Questionnaire { QuestionnaireId = 1 }, Name = "Bar" });
            session.Insert(new Booking());
            session.Insert(new Room { Name = "Room 1" });
            session.Insert(new RoomSlot { Room = new Room { RoomId = 1 } });
            session.Insert(new Bed { RoomSlot = new RoomSlot { RoomSlotId = 1 }, Booking = new Booking { BookingId = 1 } });
            session.Insert(
                new QuestionnaireResponse { Booking = new Booking { BookingId = 1 }, Questionnaire = new Questionnaire { QuestionnaireId = 1 } });
            session.Insert(
                new QuestionResponse {
                                         Question = new Question { QuestionId = 1 },
                                         QuestionnaireResponse = new QuestionnaireResponse { QuestionnaireResponseId = 1 }
                                     });
        }
    }
}