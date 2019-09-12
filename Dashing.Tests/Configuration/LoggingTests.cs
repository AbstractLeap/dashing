namespace Dashing.Tests.Configuration {
    using System;
    using System.Collections.Generic;

    using Dashing.Tests.Engine.InMemory;
    using Dashing.Tests.Engine.InMemory.TestDomain;

    public class LoggingTests {
        [Fact]
        public void WillItLog() {
            var session = this.GetSession();

            var list = session.Get<Comment>(1);

            session.Reject();
        }

        [Fact]
        public void WillItInsert() {
            var session = this.GetSession();

            var someInt = session.Insert(
                new Comment {
                                User = new User {
                                                    UserId = 1
                                                },
                                Post = new Post {
                                                    PostId = 2
                                                },
                                Content = "Test-y test comment",
                                CommentDate = DateTime.Now
                            });

            session.Reject();
        }

        private ISession GetSession() {
            var sessionCreator = new InMemoryDatabase(new ConfigurationWithLogging());
            var session = sessionCreator.BeginSession();

            var authors = new List<User> {
                                             new User {
                                                          Username = "Bob",
                                                          IsEnabled = true
                                                      },
                                             new User {
                                                          Username = "Mark",
                                                          IsEnabled = true
                                                      },
                                             new User {
                                                          Username = "James",
                                                          IsEnabled = false
                                                      }
                                         };

            session.Insert(authors);

            var blogs = new List<Blog> {
                                           new Blog {
                                                        Owner = authors[0],
                                                        Title = "Bob's Blog"
                                                    }
                                       };

            session.Insert(blogs);

            var posts = new List<Post> {
                                           new Post {
                                                        Author = authors[0],
                                                        Blog = blogs[0],
                                                        Title = "My First Post"
                                                    },
                                           new Post {
                                                        Author = authors[0],
                                                        Blog = blogs[0],
                                                        Title = "My Second Post"
                                                    },
                                           new Post {
                                                        Author = authors[1],
                                                        Title = "A post without a blog!"
                                                    }
                                       };

            session.Insert(posts);

            var comments = new List<Comment> {
                                                 new Comment {
                                                                 User = authors[1],
                                                                 Post = posts[0],
                                                                 Content = "This is marks comment on the first post"
                                                             },
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
                                                 new Comment {
                                                                 User = authors[0],
                                                                 Post = posts[0],
                                                                 Content = "This is bob's comment on the first post"
                                                             },
                                                 new Comment {
                                                                 User = authors[0],
                                                                 Post = posts[1],
                                                                 Content = "This is bob's comment on the second post"
                                                             }
                                             };
            session.Insert(comments);

            return session;
        }
    }
}