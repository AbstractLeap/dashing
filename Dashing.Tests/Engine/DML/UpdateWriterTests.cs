namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class UpdateWriterTests : IClassFixture<GenerateCodeFixture> {
        private readonly IGeneratedCodeManager codeManager;

        public UpdateWriterTests(GenerateCodeFixture data) {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void UpdateSinglePropertyWorks() {
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.PostId = 1;
            post.Title = "Boo";
            this.codeManager.TrackInstance(post);
            post.Title = "New Boo";
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var result = updateWriter.GenerateSql(new[] { post });
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [Title] = @p_1 where [PostId] = @p_2;", result.Sql);
        }

        [Fact]
        public void UpdateManyToOneProperty() {
            // assemble
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.PostId = 1;
            post.Blog = new Blog { BlogId = 1 };
            this.codeManager.TrackInstance(post);
            post.Blog = new Blog { BlogId = 2 };
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            var result = updateWriter.GenerateSql(new[] { post });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @p_1 where [PostId] = @p_2;", result.Sql); // Is this the correct result?

            var param1 = result.Parameters.GetValueOfParameter("@p_1");
            var param2 = result.Parameters.GetValueOfParameter("@p_2");

            Assert.IsType(typeof(int), param1);
            Assert.IsType(typeof(int), param2);
        }

        [Fact]
        public void UpdateSinglePropertyTwoTimes() {
            var postOne = this.codeManager.CreateTrackingInstance<Post>();
            postOne.PostId = 1;
            postOne.Title = "Boo";
            this.codeManager.TrackInstance(postOne);
            postOne.Title = "New Boo";

            var postTwo = this.codeManager.CreateTrackingInstance<Post>();
            postTwo.PostId = 1;
            postTwo.Title = "Boo";
            this.codeManager.TrackInstance(postTwo);
            postTwo.Title = "New Boo";

            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var result = updateWriter.GenerateSql(new[] { postOne, postTwo });
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [Title] = @p_1 where [PostId] = @p_2;update [Posts] set [Title] = @p_3 where [PostId] = @p_4;", result.Sql);
        }

        [Fact]
        public void UpdateTwoPropertiesWorks() {
            var target = MakeTarget();
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.PostId = 1;
            this.codeManager.TrackInstance(post);
            post.Title = "New Boo";
            post.Content = "New Content";

            // act
            var result = target.GenerateSql(new[] { post });

            // assert
            Assert.Equal("update [Posts] set [Title] = @p_1, [Content] = @p_2 where [PostId] = @p_3;", result.Sql);
        }

        [Fact(Skip = "Not true any more")]
        public void UpdateDetachedEntityWorks() {
            // assemble
            var post = new Post();
            post.PostId = 1;
            post.Title = "Boo";
            var updateWriter = MakeTarget();

            // act
            var result = updateWriter.GenerateSql(new[] { post });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal(
                "update [Posts] set [Title] = @p_1, [Content] = @p_2, [Rating] = @p_3, [AuthorId] = @p_4, [BlogId] = @p_5 where [PostId] = @p_6;",
                result.Sql);
        }

        [Fact]
        public void BuldUpdateManyOneNullAddsNull() {
            // assemble
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var updateClass = this.codeManager.CreateUpdateInstance<Post>();
            updateClass.Blog = null;

            // act
            Expression<Func<Post, bool>> predicate = p => p.PostId == 1;
            var result = updateWriter.GenerateBulkSql(updateClass, new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @Blog where ([PostId] = @l_1)", result.Sql); // Is this the correct result?

            var param1 = result.Parameters.GetValueOfParameter("@Blog");
            Assert.Null(param1);
        }

        [Fact]
        public void BulkUpdateManyToOnePropertyResolvesForeignKeyId() {
            // assemble
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var updateClass = this.codeManager.CreateUpdateInstance<Post>();
            updateClass.Blog = new Blog { BlogId = 1 };

            // act
            Expression<Func<Post, bool>> predicate = p => p.PostId == 1;
            var result = updateWriter.GenerateBulkSql(updateClass, new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @Blog where ([PostId] = @l_1)", result.Sql); // Is this the correct result?

            var param1 = result.Parameters.GetValueOfParameter("@Blog");
            var param2 = result.Parameters.GetValueOfParameter("@l_1");

            Assert.IsType(typeof(int), param1);
            Assert.IsType(typeof(int), param2);
        }

        private static UpdateWriter MakeTarget() {
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig(true));
            return updateWriter;
        }

        private static IConfiguration MakeConfig(bool withIgnore = false) {
            if (withIgnore) {
                return new CustomConfigWithIgnore();
            }

            return new CustomConfig();
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigWithIgnore : MockConfiguration {
            public CustomConfigWithIgnore() {
                this.AddNamespaceOf<Post>();
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}