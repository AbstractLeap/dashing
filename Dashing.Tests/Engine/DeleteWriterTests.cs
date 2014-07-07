namespace Dashing.Tests.Engine {
    using System.Diagnostics;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class DeleteWriterTests {
        [Fact]
        public void SingleDeleteWorks() {
            var deleteWriter = new DeleteWriter(new SqlServerDialect(), MakeConfig());
            var post = new Post { PostId = 1 };
            var result = deleteWriter.GenerateSql(new DeleteEntityQuery<Post>(post));
            Debug.Write(result.Sql);
            Assert.Equal("delete from [Posts] where [PostId] in (@p_1)", result.Sql);
        }

        [Fact]
        public void MultipleDeleteWorks() {
            var deleteWriter = new DeleteWriter(new SqlServerDialect(), MakeConfig());
            var post = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var result = deleteWriter.GenerateSql(new DeleteEntityQuery<Post>(post, post2));
            Debug.Write(result.Sql);
            Assert.Equal("delete from [Posts] where [PostId] in (@p_1, @p_2)", result.Sql);
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