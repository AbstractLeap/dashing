namespace Dashing.Tests.Engine.DML {
    using System.Diagnostics;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

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
            Assert.Equal("update [Posts] set [Title] = @p_1, [Content] = @p_2, [Rating] = @p_3, [Author] = @p_4, [Blog] = @p_5 where [PostId] = @p_6;", result.Sql);
        }

        private static UpdateWriter MakeTarget() {
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
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