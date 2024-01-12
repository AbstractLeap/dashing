namespace Dashing.Tests.Engine.InMemory {
    using System.Collections.Generic;

    using Dashing.Engine;
    using Dashing.Engine.DML;
    using Dashing.Engine.InMemory;
    using Dashing.Tests.Engine.InMemory.TestDomain;

    using Xunit;

    public class FetchClonerTests {
        [Fact]
        public void NoFetchReturnsEntityWithOnlyPrimaryKey() {
            var configuration = new TestConfiguration();
            var cloner = new FetchCloner(configuration);
            var query = (new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()));
            var fetchTreeParser = new FetchTreeParser(configuration);
            var fetchTree = fetchTreeParser.GetFetchTree(query, out _, out _);

            var post = new Post { PostId = 1, Title = "Bar", Blog = new Blog { BlogId = 2, Description = "Foo" } };
            var clone = cloner.Clone(fetchTree, post);

            Assert.False(ReferenceEquals(post, clone));
            Assert.False(ReferenceEquals(post.Blog, clone.Blog));
            Assert.False(ReferenceEquals(post.Title, clone.Title));
            Assert.Equal(1, clone.PostId);
            Assert.Equal("Bar", clone.Title);
            Assert.Equal(2, clone.Blog.BlogId);
            Assert.Null(clone.Blog.Description);
        }

        [Fact]
        public void FetchedEntityIsReturned()
        {
            var configuration = new TestConfiguration();
            var cloner = new FetchCloner(configuration);
            var query = new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Blog) as SelectQuery<Post>;
            var fetchTreeParser = new FetchTreeParser(configuration);
            var fetchTree = fetchTreeParser.GetFetchTree(query, out _, out _);

            var post = new Post { PostId = 1, Title = "Bar", Blog = new Blog { BlogId = 2, Description = "Foo" } };
            var clone = cloner.Clone(fetchTree, post);

            Assert.False(ReferenceEquals(post, clone));
            Assert.False(ReferenceEquals(post.Blog, clone.Blog));
            Assert.False(ReferenceEquals(post.Title, clone.Title));
            Assert.False(ReferenceEquals(post.Blog.Description, clone.Blog.Description));
            Assert.Equal(1, clone.PostId);
            Assert.Equal("Bar", clone.Title);
            Assert.Equal(2, clone.Blog.BlogId);
            Assert.Equal("Foo", clone.Blog.Description);
        }

        [Fact]
        public void ParentOfParentNotReturnedIfNotFetched()
        {
            var configuration = new TestConfiguration();
            var cloner = new FetchCloner(configuration);
            var query = new SelectQuery<Post>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Blog) as SelectQuery<Post>;
            var fetchTreeParser = new FetchTreeParser(configuration);
            var fetchTree = fetchTreeParser.GetFetchTree(query, out _, out _);

            var post = new Post {
                                    PostId = 1,
                                    Title = "Bar",
                                    Blog = new Blog { BlogId = 2, Description = "Foo", Owner = new User { UserId = 4, Username = "joe" } }
                                };
            var clone = cloner.Clone(fetchTree, post);

            Assert.False(ReferenceEquals(post, clone));
            Assert.False(ReferenceEquals(post.Blog, clone.Blog));
            Assert.False(ReferenceEquals(post.Title, clone.Title));
            Assert.False(ReferenceEquals(post.Blog.Description, clone.Blog.Description));
            Assert.False(ReferenceEquals(post.Blog.Owner, clone.Blog.Owner));
            Assert.Equal(1, clone.PostId);
            Assert.Equal("Bar", clone.Title);
            Assert.Equal(2, clone.Blog.BlogId);
            Assert.Equal("Foo", clone.Blog.Description);
            Assert.Equal(4, clone.Blog.Owner.UserId);
            Assert.Null(clone.Blog.Owner.Username);
        }

        [Fact]
        public void FetchedCollectionWorks()
        {
            var configuration = new TestConfiguration();
            var cloner = new FetchCloner(configuration);
            var query = new SelectQuery<Blog>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Posts) as SelectQuery<Blog>;
            var fetchTreeParser = new FetchTreeParser(configuration);
            var fetchTree = fetchTreeParser.GetFetchTree(query, out _, out _);

            var blog = new Blog {
                                    BlogId = 1,
                                    Posts = new List<Post> { new Post { PostId = 2, Title = "Foo" }, new Post { PostId = 3, Title = "Boo" } }
                                };
            var clone = cloner.Clone(fetchTree, blog);

            Assert.False(ReferenceEquals(blog, clone));
            Assert.False(ReferenceEquals(blog.Posts, clone.Posts));
            Assert.False(ReferenceEquals(blog.Posts[0], clone.Posts[0]));
            Assert.False(ReferenceEquals(blog.Posts[1], clone.Posts[1]));
            Assert.Equal("Foo", clone.Posts[0].Title);
            Assert.Equal("Boo", clone.Posts[1].Title);
        }

        [Fact]
        public void FetchedCollectionThenFetchWorks()
        {
            var configuration = new TestConfiguration();
            var cloner = new FetchCloner(configuration);
            var query =
                new SelectQuery<Blog>(new NonExecutingSelectQueryExecutor()).FetchMany(p => p.Posts).ThenFetch(p => p.Author) as SelectQuery<Blog>;
            var fetchTreeParser = new FetchTreeParser(configuration);
            var fetchTree = fetchTreeParser.GetFetchTree(query, out _, out _);

            var blog = new Blog {
                                    BlogId = 1,
                                    Posts =
                                        new List<Post> {
                                                           new Post { PostId = 2, Title = "Foo", Author = new User { UserId = 5, Username = "james" } },
                                                           new Post { PostId = 3, Title = "Boo", Author = new User { UserId = 7, Username = "mark" } }
                                                       }
                                };
            var clone = cloner.Clone(fetchTree, blog);

            Assert.False(ReferenceEquals(blog, clone));
            Assert.False(ReferenceEquals(blog.Posts, clone.Posts));
            Assert.False(ReferenceEquals(blog.Posts[0], clone.Posts[0]));
            Assert.False(ReferenceEquals(blog.Posts[1], clone.Posts[1]));
            Assert.False(ReferenceEquals(blog.Posts[0].Author, clone.Posts[0].Author));
            Assert.False(ReferenceEquals(blog.Posts[1].Author, clone.Posts[1].Author));
            Assert.Equal("Foo", clone.Posts[0].Title);
            Assert.Equal("Boo", clone.Posts[1].Title);
            Assert.Equal("james", clone.Posts[0].Author.Username);
            Assert.Equal("mark", clone.Posts[1].Author.Username);
        }

        [Fact]
        public void FetchedCollectionNoThenFetchWorks()
        {
            var configuration = new TestConfiguration();
            var cloner = new FetchCloner(configuration);
            var query = new SelectQuery<Blog>(new NonExecutingSelectQueryExecutor()).Fetch(p => p.Posts) as SelectQuery<Blog>;
            var fetchTreeParser = new FetchTreeParser(configuration);
            var fetchTree = fetchTreeParser.GetFetchTree(query, out _, out _);

            var blog = new Blog {
                                    BlogId = 1,
                                    Posts =
                                        new List<Post> {
                                                           new Post { PostId = 2, Title = "Foo", Author = new User { UserId = 5, Username = "james" } },
                                                           new Post { PostId = 3, Title = "Boo", Author = new User { UserId = 7, Username = "mark" } }
                                                       }
                                };
            var clone = cloner.Clone(fetchTree, blog);

            Assert.False(ReferenceEquals(blog, clone));
            Assert.False(ReferenceEquals(blog.Posts, clone.Posts));
            Assert.False(ReferenceEquals(blog.Posts[0], clone.Posts[0]));
            Assert.False(ReferenceEquals(blog.Posts[1], clone.Posts[1]));
            Assert.False(ReferenceEquals(blog.Posts[0].Author, clone.Posts[0].Author));
            Assert.False(ReferenceEquals(blog.Posts[1].Author, clone.Posts[1].Author));
            Assert.Equal("Foo", clone.Posts[0].Title);
            Assert.Equal("Boo", clone.Posts[1].Title);
            Assert.Equal(5, clone.Posts[0].Author.UserId);
            Assert.Equal(7, clone.Posts[1].Author.UserId);
            Assert.Null(clone.Posts[0].Author.Username);
            Assert.Null(clone.Posts[1].Author.Username);
        }
    }
}