namespace Dashing.Tests.Engine.DML {
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;
    using System;
    using Dapper;
    using System.Collections.Generic;

    public class UpdateWriterTests : IUseFixture<GenerateCodeFixture> {
        private IGeneratedCodeManager codeManager;

        public void SetFixture(GenerateCodeFixture data) {
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
            post.Blog = new Blog() { BlogId = 1 };
            this.codeManager.TrackInstance(post);
            post.Blog = new Blog() { BlogId = 2 };
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            var result = updateWriter.GenerateSql(new[] { post });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @p_1 where [PostId] = @p_2;", result.Sql); // Is this the correct result?

            var param1 = GetValueOfParameter(result.Parameters, "@p_1");
            var param2 = GetValueOfParameter(result.Parameters, "@p_2");

            Assert.IsType(typeof(int), param1);
            Assert.IsType(typeof(int), param2);
        }

        // TODO: Put this in an extension method
        private object GetValueOfParameter(DynamicParameters p, string parameterName) {
            var parametersField = typeof(DynamicParameters).GetField("parameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dynamic parameters = parametersField.GetValue(p);

            foreach (var paramInfoPair in parameters) {
                var paramInfo = paramInfoPair.GetType().GetProperty("Value").GetValue(paramInfoPair);
                var paramName = paramInfo.GetType().GetProperty("Name").GetValue(paramInfo);
                var paramValue = paramInfo.GetType().GetProperty("Value").GetValue(paramInfo);

                if (paramName == parameterName) {
                    return paramValue;
                }
            }

            return null;
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

        [Fact]
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
            Assert.Equal("update [Posts] set [Title] = @p_1, [Content] = @p_2, [Rating] = @p_3, [AuthorId] = @p_4, [BlogId] = @p_5 where [PostId] = @p_6;", result.Sql);
        }

        [Fact]
        public void BulkUpdateManyToOnePropertyResolvesForeignKeyId() {
            // assemble
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var updateClass = this.codeManager.CreateUpdateInstance<Post>();
            updateClass.Blog = new Blog() { BlogId = 1 };

            // act
            Expression<Func<Post,bool>> predicate = p => p.PostId == 1;
            var result = updateWriter.GenerateBulkSql(updateClass, new[] {predicate});

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @Blog where ([PostId] = @l_1)", result.Sql); // Is this the correct result?

            var param1 = GetValueOfParameter(result.Parameters, "@Blog");
            var param2 = GetValueOfParameter(result.Parameters, "@l_1");

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