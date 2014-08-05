namespace Dashing.Tests.Configuration.DapperMapperGeneration {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;
    using Moq;
    using Xunit;


    public class CollectionTests {
        [Fact]
        public void SingleCollectionWorks() {
            var funcFac = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var dict = new Dictionary<object, Post>();
            var func = (Func<Post, Comment, Post>)funcFac.DynamicInvoke(dict);
            func(post1, comment1);
            func(post1, comment2);
            func(post2, comment3);
            Assert.Equal(1, dict[1].Comments.First().CommentId);
            Assert.Equal(2, dict[1].Comments.Last().CommentId);
            Assert.Equal(3, dict[2].Comments.First().CommentId);
        }

        [Fact]
        public void SingleCollectionAwkwardObjectWorks() {
            var funcFac = GenerateSingleAwkwardMapper();
            var post1 = new PostWithoutCollectionInitializerInConstructor { PostWithoutCollectionInitializerInConstructorId = 1 };
            var post2 = new PostWithoutCollectionInitializerInConstructor { PostWithoutCollectionInitializerInConstructorId = 2 };
            var comment1 = new CommentTwo { CommentTwoId = 1 };
            var comment2 = new CommentTwo { CommentTwoId = 2 };
            var comment3 = new CommentTwo { CommentTwoId = 3 };
            var dict = new Dictionary<object, PostWithoutCollectionInitializerInConstructor>();
            var func = (Func<PostWithoutCollectionInitializerInConstructor, CommentTwo, PostWithoutCollectionInitializerInConstructor>)funcFac.DynamicInvoke(dict);
            func(post1, comment1);
            func(post1, comment2);
            func(post2, comment3);
            Assert.Equal(1, dict[1].Comments.First().CommentTwoId);
            Assert.Equal(2, dict[1].Comments.Last().CommentTwoId);
            Assert.Equal(3, dict[2].Comments.First().CommentTwoId);
        }

        [Fact]
        public void MultiCollectionWorks() {
            var funcFac = GenerateMultiMapper();

            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var postTag1 = new PostTag { PostTagId = 1 };
            var postTag2 = new PostTag { PostTagId = 2 };
            var postTag3 = new PostTag { PostTagId = 3 };
            var otherDict = new Dictionary<string, IDictionary<object, object>>();
            otherDict.Add("fetchParam_1", new Dictionary<object, object>());
            otherDict.Add("fetchParam_2", new Dictionary<object, object>());
            var dict = new Dictionary<object, Post>();
            var func = (Func<Post, Comment, PostTag, Post>)funcFac.DynamicInvoke(dict, otherDict);
            func(post1, comment1, postTag1);
            func(post1, comment2, postTag1);
            func(post2, comment3, postTag2);
            func(post2, comment3, postTag3);

            Assert.Equal(1, dict[1].Comments.First().CommentId);
            Assert.Equal(2, dict[1].Comments.Last().CommentId);
            Assert.Equal(2, dict[1].Comments.Count);

            Assert.Equal(3, dict[2].Comments.First().CommentId);
            Assert.Equal(1, dict[2].Comments.Count);

            Assert.Equal(1, dict[1].Tags.First().PostTagId);
            Assert.Equal(1, dict[1].Tags.Count);

            Assert.Equal(2, dict[2].Tags.First().PostTagId);
            Assert.Equal(3, dict[2].Tags.Last().PostTagId);
            Assert.Equal(2, dict[2].Tags.Count);
        }

        private static Delegate GenerateMultiMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(config.Engine, new Mock<IDbTransaction>().Object).Fetch(p => p.Comments).Fetch(p => p.Tags) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object);
            var func = mapper.GenerateMultiCollectionMapper<Post>(result.FetchTree, false);
            return func;
        }

        private static Delegate GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(config.Engine, new Mock<IDbTransaction>().Object).Fetch(p => p.Comments) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree, false);
            return func;
        }

        private static Delegate GenerateSingleAwkwardMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<PostWithoutCollectionInitializerInConstructor>(config.Engine, new Mock<IDbTransaction>().Object).Fetch(p => p.Comments) as SelectQuery<PostWithoutCollectionInitializerInConstructor>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object);
            var func = mapper.GenerateCollectionMapper<PostWithoutCollectionInitializerInConstructor>(result.FetchTree, false);
            return func;
        }

        private static Mock<IGeneratedCodeManager> GetMockCodeManager() {
            var mockCodeManager = new Mock<IGeneratedCodeManager>(MockBehavior.Strict);
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Post))).Returns(typeof(Post));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Blog))).Returns(typeof(Blog));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Comment))).Returns(typeof(Comment));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(User))).Returns(typeof(User));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Tag))).Returns(typeof(Tag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(PostTag))).Returns(typeof(PostTag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(PostWithoutCollectionInitializerInConstructor))).Returns(typeof(PostWithoutCollectionInitializerInConstructor));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(CommentTwo))).Returns(typeof(CommentTwo));

            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Post)))).Returns(typeof(Post));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Blog)))).Returns(typeof(Blog));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Comment)))).Returns(typeof(Comment));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(User)))).Returns(typeof(User));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Tag)))).Returns(typeof(Tag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(PostTag)))).Returns(typeof(PostTag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(PostWithoutCollectionInitializerInConstructor)))).Returns(typeof(PostWithoutCollectionInitializerInConstructor));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(CommentTwo)))).Returns(typeof(CommentTwo));
            return mockCodeManager;
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new System.Configuration.ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                this.AddNamespaceOf<Post>();
                this.Add<PostWithoutCollectionInitializerInConstructor>();
                this.Add<CommentTwo>();
            }
        }
    }

    public class PostWithoutCollectionInitializerInConstructor {
        public virtual int PostWithoutCollectionInitializerInConstructorId { get; set; }

        public virtual ICollection<CommentTwo> Comments { get; set; }
    }

    public class CommentTwo {
        public virtual int CommentTwoId { get; set; }

        public virtual PostWithoutCollectionInitializerInConstructor PostWithoutCollectionInitializerInConstructor { get; set; }
    }
}
