namespace Dashing.Tests.Engine.DapperMapperGeneration {
    using System;
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class NonCollectionTests {
        [Fact]
        public void SingleFetchWorks() {
            var func = GenerateSingleMapper();

            var post1 = new Post { PostId = 1 };
            var blog1 = new Blog { BlogId = 2 };
            var post = ((Func<object[], Post>)func)(new object[] { post1, blog1 });
            Assert.Equal(2, post.Blog.BlogId);
        }

        [Fact]
        public void SingleFetchWithNullWorks() {
            var func = GenerateSingleMapper();
            var post = new Post { PostId = 1 };
            Blog blog = null;
            var result = ((Func<object[], Post>)func)(new object[] { post, blog });
            Assert.Null(post.Blog);
        }

        [Fact]
        public void FetchedEntitiesHaveTrackingEnabled() {
            var func = GenerateSingleMapper();
            var post = new Post { PostId = 1 };
            var blog = new Blog { BlogId = 2 };
            var result = ((Func<object[], Post>)func)(new object[] { post, blog });
            Assert.True(((ITrackedEntity)result).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)result.Blog).IsTrackingEnabled());
        }

        [Fact]
        public void EnableTrackingCalledLast() {
            var func = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var blog1 = new Blog { BlogId = 2 };
            var post = ((Func<object[], Post>)func)(new object[] { post1, blog1 });
            Assert.False(((ITrackedEntity)post).GetDirtyProperties().Any());
        }

        private static Delegate GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Blog) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new NonCollectionMapperGenerator(config);
            var func = mapper.GenerateNonCollectionMapper<Post>(result.FetchTree);
            return func.Item1;
        }

        [Fact]
        public void MultiFetchNoCollectionWorks() {
            var func = GenerateMultipleNoCollectionMapper();
            var comment = new Comment();
            var post = new Post { PostId = 1 };
            var author = new User { UserId = 3 };
            var resultComment = ((Func<object[], Comment>)func)(new object[] { comment, post, author });
            Assert.Equal(3, resultComment.Post.Author.UserId);
        }

        [Fact]
        public void MultiFetchNoCollectionHasTrackingEnabled() {
            var func = GenerateMultipleNoCollectionMapper();
            var comment = new Comment();
            var post = new Post { PostId = 1 };
            var author = new User { UserId = 3 };
            var resultComment = ((Func<object[], Comment>)func)(new object[] { comment, post, author });
            Assert.True(((ITrackedEntity)resultComment).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)resultComment.Post).IsTrackingEnabled());
            Assert.True(((ITrackedEntity)resultComment.Post.Author).IsTrackingEnabled());
        }

        [Fact]
        public void MultiFetchNoCollectionHasTrackingEnabledLast() {
            var func = GenerateMultipleNoCollectionMapper();
            var comment = new Comment();
            var post = new Post { PostId = 1 };
            var author = new User { UserId = 3 };
            var resultComment = ((Func<object[], Comment>)func)(new object[] { comment, post, author });
            Assert.False(((ITrackedEntity)resultComment).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)resultComment.Post).GetDirtyProperties().Any());
            Assert.False(((ITrackedEntity)resultComment.Post.Author).GetDirtyProperties().Any());
        }

        [Fact]
        public void MultiFetchNoCollectionNullWorks() {
            var func = GenerateMultipleNoCollectionMapper();
            var comment = new Comment { CommentId = 1 };
            Post post = null;
            var author = new User { UserId = 3 };
            var resultComment = ((Func<object[], Comment>)func)(new object[] { comment, post, author });
            Assert.Equal(1, resultComment.CommentId);
            Assert.Null(resultComment.Post);
        }

        private static Delegate GenerateMultipleNoCollectionMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Comment>(new Mock<ISelectQueryExecutor>().Object).Fetch(c => c.Post.Author) as SelectQuery<Comment>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery, new AutoNamingDynamicParameters());
            var mapper = new NonCollectionMapperGenerator(config);
            var func = mapper.GenerateNonCollectionMapper<Comment>(result.FetchTree);
            return func.Item1;
        }

        private class CustomConfig : BaseConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}