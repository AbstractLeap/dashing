namespace Dashing.Tests.Configuration.DapperMapperGeneration {
    using System;
    using System.Configuration;
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
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

        private static Delegate GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<IExecuteSelectQueries>().Object).Fetch(p => p.Blog) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);
            var mockCodeManager = GetMockCodeManager();
            var mapper = new DapperMapperGenerator(mockCodeManager.Object);
            var func = mapper.GenerateNonCollectionMapper<Post>(result.FetchTree, false);
            return func.Item1;
        }

        private static Mock<IGeneratedCodeManager> GetMockCodeManager() {
            var mockCodeManager = new Mock<IGeneratedCodeManager>();
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Post))).Returns(typeof(Post));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Blog))).Returns(typeof(Blog));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Comment))).Returns(typeof(Comment));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(User))).Returns(typeof(User));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Post)))).Returns(typeof(Post));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Blog)))).Returns(typeof(Blog));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Comment)))).Returns(typeof(Comment));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(User)))).Returns(typeof(User));
            return mockCodeManager;
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
            var selectQuery = new SelectQuery<Comment>(new Mock<IExecuteSelectQueries>().Object).Fetch(c => c.Post.Author) as SelectQuery<Comment>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);
            var mockCodeManager = GetMockCodeManager();
            var mapper = new DapperMapperGenerator(mockCodeManager.Object);
            var func = mapper.GenerateNonCollectionMapper<Comment>(result.FetchTree, false);
            return func.Item1;
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}