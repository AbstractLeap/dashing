namespace Dashing.Tests.Engine {
    using Moq;
    using System.Diagnostics;
    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Tests.TestDomain;
    using Xunit;

    public class InsertWriterTests {
        [Fact]
        public void SimpleInsertWorks() {
            var insertWriter = new InsertWriter(new SqlServerDialect(), MakeConfig());
            var post = new Post { PostId = 1, Title = "Boo", Rating = 11 };
            var result = insertWriter.GenerateSql(post);
            Debug.Write(result.Sql);
            Assert.Equal("insert into [Posts] ([Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap]) values (@p_1, @p_2, @p_3, @p_4, @p_5, @p_6)", result.Sql);
        }

        private static IConfiguration MakeConfig(bool withIgnore = false) {
            if (withIgnore) {
                return new CustomConfigWithIgnore();
            }

            return new CustomConfig();
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new Mock<IEngine>().Object, string.Empty) {
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