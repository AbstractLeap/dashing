namespace TopHat.Tests.Engine {
    using Moq;
    using System.Diagnostics;
    using TopHat.Configuration;
    using TopHat.Engine;
    using TopHat.Tests.TestDomain;
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

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new Mock<IEngine>().Object, string.Empty)
            {
                this.AddNamespaceOf<Post>();
            }
        }

        private class CustomConfigWithIgnore : DefaultConfiguration {
            public CustomConfigWithIgnore()
                : base(new Mock<IEngine>().Object, string.Empty)
            {
                this.AddNamespaceOf<Post>();
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}