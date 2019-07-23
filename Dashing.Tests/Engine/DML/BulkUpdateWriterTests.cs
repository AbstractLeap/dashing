namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.Extensions;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.Constructor;

    using Xunit;
    using Xunit.Abstractions;

    public class BulkUpdateWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public BulkUpdateWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void BulkUpdateManyOneNullAddsNull() {
            // assemble
            var updateWriter = new BulkUpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            Expression<Func<Post, bool>> predicate = p => p.PostId == 1;
            var result = updateWriter.GenerateBulkSql(p => p.Blog = null, new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @Blog where ([PostId] = @l_1)", result.Sql); // Is this the correct result?

            var param1 = result.Parameters.GetValueOfParameter("@Blog");
            Assert.Null(param1);
        }

        [Fact]
        public void BulkUpdateManyToOnePropertyResolvesForeignKeyId() {
            // assemble
            var updateWriter = new BulkUpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            Expression<Func<Post, bool>> predicate = p => p.PostId == 1;
            var result = updateWriter.GenerateBulkSql(
                p => p.Blog = new Blog {
                                           BlogId = 1
                                       },
                new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @Blog where ([PostId] = @l_1)", result.Sql); // Is this the correct result?

            var param1 = result.Parameters.GetValueOfParameter("@Blog");
            var param2 = result.Parameters.GetValueOfParameter("@l_1");

            Assert.IsType<int>(param1);
            Assert.IsType<int>(param2);
        }

        [Fact]
        public void BulkUpdateIgnoresConstructorSetProperties() {
            // assemble
            var updateWriter = new BulkUpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            Expression<Func<ClassWithConstructor, bool>> predicate = p => p.Id == 1;
            var result = updateWriter.GenerateBulkSql(p => { }, new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal(string.Empty, result.Sql); // Is this the correct result?
        }

        [Fact]
        public void MultipleJoinFkWorks() {
            var updateWriter = new BulkUpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            var author = new User {
                                      UserId = 99
                                  };
            Expression<Func<Post, bool>> predicate = p => p.Blog.Owner == author;
            var result = updateWriter.GenerateBulkSql(
                p => p.Blog = new Blog {
                                           BlogId = 1
                                       },
                new[] { predicate });

            // assert
            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("update t set t.[BlogId] = @Blog from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[OwnerId] = @l_1)", result.Sql);
            Assert.Equal(99, result.Parameters.Get<int>("l_1"));
            Assert.Equal(1, result.Parameters.Get<int>("Blog"));
        }

        [Fact]
        public void MultipleJoinWorks() {
            var updateWriter = new BulkUpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            Expression<Func<Post, bool>> predicate = p => p.Blog.Owner.EmailAddress.EndsWith("@acme.com");
            var result = updateWriter.GenerateBulkSql(
                p => {
                    p.Blog = new Blog {
                                          BlogId = 1
                                      };
                    p.Rating = 5;
                },
                new[] { predicate });

            // assert
            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("update t set t.[Rating] = @Rating, t.[BlogId] = @Blog from [Posts] as t left join [Blogs] as t_100 on t.BlogId = t_100.BlogId left join [Users] as t_101 on t_100.OwnerId = t_101.UserId where t_101.[EmailAddress] like @l_1", result.Sql);
            Assert.Equal("%@acme.com", result.Parameters.Get<string>("l_1"));
            Assert.Equal(1, result.Parameters.Get<int>("Blog"));
            Assert.Equal(5m, result.Parameters.Get<decimal>("Rating"));
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
                this.AddNamespaceOf<ClassWithConstructor>();
            }
        }

        private class CustomConfigWithIgnore : MockConfiguration {
            public CustomConfigWithIgnore() {
                this.AddNamespaceOf<Post>();
                this.AddNamespaceOf<ClassWithConstructor>();
                this.Setup<Post>()
                    .Property(p => p.DoNotMap)
                    .Ignore();
            }
        }
    }
}