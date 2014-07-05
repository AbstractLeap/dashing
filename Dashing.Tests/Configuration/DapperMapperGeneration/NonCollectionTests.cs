﻿namespace Dashing.Tests.Configuration.DapperMapperGeneration {
    using System;
    using System.Configuration;
    using System.Data;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class NonCollectionTests {
        [Fact]
        public void SingleFetchWorks() {
            var func = GenerateSingleMapper();

            var post1 = new Post { PostId = 1 };
            var blog1 = new Blog { BlogId = 2 };
            var post = ((Func<Post, Blog, Post>)func)(post1, blog1);
            Assert.Equal(2, post.Blog.BlogId);
        }

        [Fact]
        public void SingleFetchWithNullWorks() {
            var func = GenerateSingleMapper();
            Post post = null;
            var blog = new Blog { BlogId = 2 };
            var result = ((Func<Post, Blog, Post>)func)(post, blog);
            Assert.Null(result);
        }

        private static Delegate GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(config.Engine, new Mock<IDbConnection>().Object).Fetch(p => p.Blog) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(new Mock<IGeneratedCodeManager>().Object);
            var func = mapper.GenerateNonCollectionMapper<Post>(result.FetchTree, false);
            return func;
        }

        [Fact]
        public void MultiFetchNoCollectionWorks() {
            var func = GenerateMultipleNoCollectionMapper();
            var comment = new Comment();
            var post = new Post { PostId = 1 };
            var author = new User { UserId = 3 };
            var resultComment = ((Func<Comment, Post, User, Comment>)func)(comment, post, author);
            Assert.Equal(3, resultComment.Post.Author.UserId);
        }

        [Fact]
        public void MultiFetchNoCollectionNullWorks() {
            var func = GenerateMultipleNoCollectionMapper();
            var comment = new Comment { CommentId = 1 };
            Post post = null;
            var author = new User { UserId = 3 };
            var resultComment = ((Func<Comment, Post, User, Comment>)func)(comment, post, author);
            Assert.Equal(1, resultComment.CommentId);
            Assert.Null(resultComment.Post);
        }

        private static Delegate GenerateMultipleNoCollectionMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Comment>(config.Engine, new Mock<IDbConnection>().Object).Fetch(c => c.Post.Author) as SelectQuery<Comment>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(new Mock<IGeneratedCodeManager>().Object);
            var func = mapper.GenerateNonCollectionMapper<Comment>(result.FetchTree, false);
            return func;
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}