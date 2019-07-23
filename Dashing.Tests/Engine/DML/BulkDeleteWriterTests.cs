namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Xunit;
    using Xunit.Abstractions;

    public class BulkDeleteWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public BulkDeleteWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void SimplePredicateWorks() {
            var bulkDeleteWriter = new BulkDeleteWriter(new SqlServerDialect(), MakeConfig());
            var result = bulkDeleteWriter.GenerateBulkSql(new Expression<Func<Post, bool>>[] { p => p.Title == "Foo" });
            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("delete from [Posts] where ([Title] = @l_1)", result.Sql);
            Assert.Equal("Foo", result.Parameters.Get<string>("l_1"));
        }

        [Fact]
        public void ForeignKeyWorks() {
            var bulkDeleteWriter = new BulkDeleteWriter(new SqlServerDialect(), MakeConfig());
            var author = new User { UserId = 99 };
            var result = bulkDeleteWriter.GenerateBulkSql(new Expression<Func<Post, bool>>[] { p => p.Author == author });
            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("delete from [Posts] where ([AuthorId] = @l_1)", result.Sql);
            Assert.Equal(99, result.Parameters.Get<int>("l_1"));
        }

        [Fact]
        public void MultipleJoinFkWorks() {
            var bulkDeleteWriter = new BulkDeleteWriter(new SqlServerDialect(), MakeConfig());
            var author = new User { UserId = 98 };
            var result = bulkDeleteWriter.GenerateBulkSql(new Expression<Func<Post, bool>>[] { p => p.Blog.Owner == author });
            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("delete t from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[OwnerId] = @l_1)", result.Sql);
            Assert.Equal(98, result.Parameters.Get<int>("l_1"));
        }

        [Fact]
        public void MultipleJoinWorks() {
            var bulkDeleteWriter = new BulkDeleteWriter(new SqlServerDialect(), MakeConfig());
            var author = new User { UserId = 98 };
            var result = bulkDeleteWriter.GenerateBulkSql(new Expression<Func<Post, bool>>[] { p => p.Blog.Owner.EmailAddress.EndsWith("@acme.com") });
            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("delete t from [Posts] as t left join [Blogs] as t_100 on t.BlogId = t_100.BlogId left join [Users] as t_101 on t_100.OwnerId = t_101.UserId where t_101.[EmailAddress] like @l_1", result.Sql);
            Assert.Equal("%@acme.com", result.Parameters.Get<string>("l_1"));
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
                this.Setup<Post>()
                    .Property(p => p.DoNotMap)
                    .Ignore();
            }
        }
    }
}