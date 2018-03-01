namespace Dashing.Tests.Engine.InMemory {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Engine.InMemory;
    using Dashing.Tests.Engine.InMemory.TestDomain;

    using Xunit;

    public class EntityClonerTests {
        [Fact]
        public void CloneWorks() {
            var cloner = new EntityCloner<Post>(new TestConfiguration());
            var post = new Post() { PostId = 1, Title = "Foo", Blog = new Blog { BlogId = 1, Title = "My Blog" }};
            var clone = cloner.Clone(post);

            Assert.False(post == clone);
            Assert.True(post.Equals(clone));
            Assert.False(ReferenceEquals(post.Title, clone.Title));
            Assert.Equal(post.PostId, clone.PostId);
            Assert.True(post.Blog.Equals(clone.Blog));
            Assert.False(post.Blog == clone.Blog);
            Assert.Null(clone.Blog.Title);
            Assert.Equal("My Blog", post.Blog.Title);
        }

        [Fact]
        public void CloneRemovesCollections() {
            var cloner = new EntityCloner<Post>(new TestConfiguration());
            var post = new Post() { PostId = 1, Title = "Foo", Comments = new List<Comment> {
                                                                                                new Comment { CommentId = 1, Post = new Post() { PostId = 1 }}
                                                                                            }};
            var clone = cloner.Clone(post);
            Assert.True(clone.Comments == null || !clone.Comments.Any());
        }

        [Fact]
        public void IgnoredPropDoesNotGetCloned() {
            var cloner = new EntityCloner<Post>(new TestConfiguration());
            var post = new Post() {
                PostId = 1,
                Title = "Foo",
                DoNotMap = true
            };
            var clone = cloner.Clone(post);
            Assert.False(clone.DoNotMap);
        }
    }
}