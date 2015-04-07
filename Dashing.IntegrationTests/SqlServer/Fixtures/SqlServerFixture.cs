namespace Dashing.IntegrationTests.SqlServer.Fixtures {
    using System;
    using System.Collections.Generic;

    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.Tools.Migration;

    public class SqlServerFixture : IDisposable {
        public ISession Session { get; set; }

        public string DatabaseName { get; private set; }

        private readonly IConfiguration config;

        public SqlServerFixture() {
            this.config = new SqlServerConfiguration();
            this.DatabaseName = "DashingIntegration_" + Guid.NewGuid().ToString("D").Substring(0, 8);

            // load the data
            using (var transactionLessSession = this.config.BeginTransactionLessSession()) {
            var migrator = new Migrator(
                this.config.Engine.SqlDialect,
                new CreateTableWriter(this.config.Engine.SqlDialect),
                new AlterTableWriter(this.config.Engine.SqlDialect),
                new DropTableWriter(this.config.Engine.SqlDialect),
                new StatisticsProvider(null, this.config.Engine.SqlDialect));
            IEnumerable<string> warnings, errors;
            var createStatement = migrator.GenerateSqlDiff(new List<IMap>(), this.config.Maps, null, null, new string[0], out warnings, out errors);
                transactionLessSession.Dapper.Execute("create database " + this.DatabaseName);
                transactionLessSession.Dapper.Execute("use " + this.DatabaseName);
                transactionLessSession.Dapper.Execute(createStatement);
            }

            this.Session = this.config.BeginSession();
            this.Session.Dapper.Execute("use " + this.DatabaseName);
            this.InsertData();
        }

        private void InsertData() {
            var users = new List<User>();
            for (var i = 0; i < 10; i++) {
                var user = new User { Username = "User_" + i };
                users.Add(user);
                this.Session.Insert(user);
            }

            var blogs = new List<Blog>();
            for (var i = 0; i < 10; i++) {
                var blog = new Blog { Title = "Blog_" + i };
                blogs.Add(blog);
                this.Session.Insert(blog);
            }

            var posts = new List<Post>();
            for (var i = 0; i < 20; i++) {
                var userId = i / 2;
                var blogId = i / 2;
                var post = new Post { Author = users[userId], Blog = blogs[blogId], Title = "Post_" + i };
                this.Session.Insert(post);
                posts.Add(post);
            }

            for (var i = 0; i < 30; i++) {
                var comment = new Comment { Post = posts[i / 2], User = users[i / 3], Content = "Comment_" + i };
                this.Session.Insert(comment);
            }

            var tags = new List<Tag>();
            for (var i = 0; i < 20; i++) {
                var tag = new Tag { Content = "Tag_" + i };
                tags.Add(tag);
                this.Session.Insert(tag);
            }

            for (var i = 0; i < 30; i++) {
                var postTag = new PostTag { Post = posts[i / 2], Tag = tags[i / 2] };
                this.Session.Insert(postTag);
            }

            // insert single comment with null User to check that nulls are returned properly
            var nullUserComment = new Comment { Post = posts[0], User = null, Content = "Nullable User Content" };
            this.Session.Insert(nullUserComment);

            var nullTag = new Tag { Content = "Null Post Tag" };
            this.Session.Insert(nullTag);
            var nullPostTag = new PostTag { Tag = nullTag };
            this.Session.Insert(nullPostTag);

            // add user for bulk update
            this.Session.Insert(new User { Username = "BulkUpdate", Password = "Blah" });

            // add users for bulk delete
            this.Session.Insert(new User { Username = "BulkDelete", Password = "Foo" }, new User { Username = "BulkDelete", Password = "Bar" });

            // test delete user
            this.Session.Insert(new User { Username = "TestDelete", Password = "Blah" });

            // test empty collection
            this.Session.Insert(new Blog { Title = "EmptyBlog" });
        }

        public void Dispose() {
            this.Session.Dispose();
            using (var transactionLessSession = this.config.BeginTransactionLessSession()) {
                transactionLessSession.Dapper.Execute("drop database " + this.DatabaseName);
            }
        }
    }
}