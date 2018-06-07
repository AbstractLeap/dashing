namespace Dashing.Tests.Engine.DML {
    using System.Diagnostics;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.Versioning;

    using Xunit;

    public class InsertWriterTests {
        [Fact]
        public void SimpleInsertWorks() {
            var insertWriter = new InsertWriter(new SqlServerDialect(), MakeConfig());
            var post = new Post { PostId = 1, Title = "Boo", Rating = 11 };
            var result = insertWriter.GenerateSql(post);
            Debug.Write(result.Sql);
            Assert.Equal(
                "insert into [Posts] ([AuthorId], [BlogId], [Content], [DoNotMap], [Rating], [Title]) output inserted.[PostId] values (@p_1, @p_2, @p_3, @p_4, @p_5, @p_6)",
                result.Sql);
        }

        [Fact]
        public void InsertingVersionedEntities() {
            var insertWriter = new InsertWriter(new SqlServerDialect(), new VersionedConfig());
            var entity = new VersionedEntity { Name = "The Baz Man" };
            var result = insertWriter.GenerateSql(entity);

            Assert.DoesNotContain(nameof(VersionedEntity.SysEndTime), result.Sql);
            Assert.DoesNotContain(nameof(VersionedEntity.SysStartTime), result.Sql);
            Assert.DoesNotContain(nameof(VersionedEntity.SessionUser), result.Sql);
            Assert.DoesNotContain(nameof(VersionedEntity.CreatedBy), result.Sql);
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

        private class VersionedConfig : MockConfiguration {
            public VersionedConfig() {
                this.AddNamespaceOf<VersionedEntity>();
            }
        }
    }
}