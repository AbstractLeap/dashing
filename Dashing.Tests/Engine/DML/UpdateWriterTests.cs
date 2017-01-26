namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.Extensions;
    using Dashing.Tests.TestDomain;
    using Dashing.Tests.TestDomain.Constructor;

    using Xunit;

    public class UpdateWriterTests {
        [Fact]
        public void UpdateSinglePropertyWorks() {
            var post = new Post();
            post.PostId = 1;
            post.Title = "Boo";
            ((ITrackedEntity)post).EnableTracking();
            post.Title = "New Boo";
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var result = updateWriter.GenerateSql(new[] { post });
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [Title] = @p_1 where [PostId] = @p_2;", result.Sql);
        }

        [Fact]
        public void UpdateManyToOneProperty() {
            // assemble
            var post = new Post();
            post.PostId = 1;
            post.Blog = new Blog { BlogId = 1 };
            ((ITrackedEntity)post).EnableTracking();
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
            var postOne = new Post();
            postOne.PostId = 1;
            postOne.Title = "Boo";
            ((ITrackedEntity)postOne).EnableTracking();
            postOne.Title = "New Boo";

            var postTwo = new Post();
            postTwo.PostId = 1;
            postTwo.Title = "Boo";
            ((ITrackedEntity)postTwo).EnableTracking();
            postTwo.Title = "New Boo";

            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());
            var result = updateWriter.GenerateSql(new[] { postOne, postTwo });
            Debug.Write(result.Sql);
            Assert.Equal(
                "update [Posts] set [Title] = @p_1 where [PostId] = @p_2;update [Posts] set [Title] = @p_3 where [PostId] = @p_4;",
                result.Sql);
        }

        [Fact]
        public void UpdateTwoPropertiesWorks() {
            var target = MakeTarget();
            var post = new Post();
            post.PostId = 1;
            ((ITrackedEntity)post).EnableTracking();
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
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            Expression<Func<Post, bool>> predicate = p => p.PostId == 1;
            var result = updateWriter.GenerateBulkSql(p => p.Blog = new Blog { BlogId = 1 }, new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal("update [Posts] set [BlogId] = @Blog where ([PostId] = @l_1)", result.Sql); // Is this the correct result?

            var param1 = result.Parameters.GetValueOfParameter("@Blog");
            var param2 = result.Parameters.GetValueOfParameter("@l_1");

            Assert.IsType(typeof(int), param1);
            Assert.IsType(typeof(int), param2);
        }

        [Fact]
        public void BulkUpdateIgnoresConstructorSetProperties() {
            // assemble
            var updateWriter = new UpdateWriter(new SqlServerDialect(), MakeConfig());

            // act
            Expression<Func<ClassWithConstructor, bool>> predicate = p => p.Id == 1;
            var result = updateWriter.GenerateBulkSql(p => { }, new[] { predicate });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal(string.Empty, result.Sql); // Is this the correct result?
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
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }
    }
}