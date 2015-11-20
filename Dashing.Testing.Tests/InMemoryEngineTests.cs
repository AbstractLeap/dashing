namespace Dashing.Testing.Tests {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Testing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class InMemoryEngineTests {
        [Fact]
        public void ItWorks() {
            var session = this.GetSession();
            var firstComment = session.Query<Comment>().Fetch(c => c.Post.Author).Single(c => c.CommentId == 1);
            Assert.NotNull(firstComment);
            Assert.Equal(1, firstComment.Post.PostId);
            Assert.Equal(1, firstComment.Post.Author.UserId);
            Assert.Equal(2, firstComment.User.UserId);
            Assert.Null(firstComment.User.Username);
            Assert.Equal("Bob", firstComment.Post.Author.Username);
            Assert.Equal("My First Post", firstComment.Post.Title);
        }

        [Fact]
        public void CollectionsWork() {
            var session = this.GetSession();
            var posts = session.Query<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User).ToArray();
            Assert.Equal(2, posts.Length);
            Assert.Equal(3, posts.First().Comments.Count);
            Assert.Equal("Mark", posts.First().Comments.First().User.Username);
            Assert.Equal(posts.First().Comments.First().Post.PostId, posts.First().PostId);
            Assert.Null(posts.First().Comments.First().Post.Title);
            Assert.NotNull(posts.First().Title);
        }

        [Fact]
        public void TestConfigWorks() {
            var config = new TestConfiguration(true);
            using (var session = config.BeginSession()) {
                session.Insert(new Post() { Title = "Foo" });
                Assert.Equal("Foo", session.Get<Post>(1).Title);
            }
        }

        private ISession GetSession() {
            var engine = new InMemoryEngine() { Configuration = new TestConfiguration() };
            var session = new Session(engine, new Mock<ISessionState>().Object);
            
            var authors = new List<User> { new User { Username = "Bob" }, new User { Username = "Mark" }, new User { Username = "James" } };
            session.Insert(authors);
            
            var blogs = new List<Blog> { new Blog { Owner = authors[0], Title = "Bob's Blog" } };
            session.Insert(blogs);

            var posts = new List<Post> {
                                           new Post { Author = authors[0], Blog = blogs[0], Title = "My First Post" },
                                           new Post { Author = authors[0], Blog = blogs[0], Title = "My Second Post" }
                                       };
            session.Insert(posts);

            var comments = new List<Comment> {
                                                 new Comment { User = authors[1], Post = posts[0], Content = "This is marks comment on the first post" },
                                                 new Comment {
                                                                 User = authors[1],
                                                                 Post = posts[1],
                                                                 Content = "This is marks comment on the second post"
                                                             },
                                                 new Comment {
                                                                 User = authors[2],
                                                                 Post = posts[0],
                                                                 Content = "This is james' comment on the first post"
                                                             },
                                                 new Comment {
                                                                 User = authors[2],
                                                                 Post = posts[1],
                                                                 Content = "This is james' comment on the second post"
                                                             },
                                                 new Comment { User = authors[0], Post = posts[0], Content = "This is bob's comment on the first post" },
                                                 new Comment {
                                                                 User = authors[0],
                                                                 Post = posts[1],
                                                                 Content = "This is bob's comment on the second post"
                                                             },
                                             };
            session.Insert(comments);
            return session;
        }
    }
}