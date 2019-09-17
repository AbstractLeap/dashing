namespace Dashing.Tests.CodeGeneration {
    using System;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class ProjectExpressionRewriterTests {
        private readonly ITestOutputHelper outputHelper;

        public ProjectExpressionRewriterTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void SelectAnonymousWorks()
        {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new {
                                    p.Title
                                });
            this.AssertMapperMatches(new []{ new Post { Title = "Foo" }}
                , query, 
                obj => Assert.Equal("Foo", obj.Title));
        }

        [Fact]
        public void SelectBaseWorks()
        {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new Post
                                {
                                    Title = p.Title
                                });
            this.AssertMapperMatches(new[] { new Post { Title = "Foo" } }, 
                query, 
                obj => Assert.Equal("Foo", obj.Title),
                obj => Assert.IsType<Post>(obj));
        }

        [Fact]
        public void FetchProjectBaseWorks()
        {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new Post
                                {
                                    Title = p.Title,
                                    Author = p.Author
                                });
            this.AssertMapperMatches(new object[] { new Post { Title = "Foo" }, new User { UserId = 35, EmailAddress = "joe@acme.com" } },
                query,
                obj => Assert.Equal("Foo", obj.Title),
                obj => Assert.Equal(35, obj.Author.UserId),
                obj => Assert.Equal("joe@acme.com", obj.Author.EmailAddress));
        }

        [Fact]
        public void FetchProjectAnonymousWorks()
        {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new
                                {
                                    Title = p.Title,
                                    Author = p.Author
                                });
            this.AssertMapperMatches(new object[] { new Post { Title = "Foo" }, new User { UserId = 35, EmailAddress = "joe@acme.com" } },
                query,
                obj => Assert.Equal("Foo", obj.Title),
                obj => Assert.Equal(35, obj.Author.UserId),
                obj => Assert.Equal("joe@acme.com", obj.Author.EmailAddress));
        }

        [Fact]
        public void ProjectRelationshipAnonymousWorks()
        {
            var query = this.GetSelectQuery<Post>()
                            .Select(
                                p => new
                                     {
                                         Title = p.Title,
                                         AuthorId = p.Author.UserId,
                                         EmailAddress = p.Author.EmailAddress
                                     });
            this.AssertMapperMatches(new object[] { new Post { Title = "Foo" }, new User { UserId = 35, EmailAddress = "joe@acme.com" } },
                query,
                obj => Assert.Equal("Foo", obj.Title),
                obj => Assert.Equal(35, obj.AuthorId),
                obj => Assert.Equal("joe@acme.com", obj.EmailAddress));
        }

        private void AssertMapperMatches<TBase, TProjection>(object[] inputs, IProjectedSelectQuery<TBase, TProjection> projectedSelectQuery, params Action<TProjection>[] assertions)
            where TBase : class, new()
        {
            var selectWriter = this.GetSql2012Writer();
            var concreteQuery = (ProjectedSelectQuery<TBase, TProjection>)projectedSelectQuery;
            var sqlResult = selectWriter.GenerateSql(concreteQuery);
            var projectionRewriter = new ProjectionExpressionRewriter<TBase, TProjection>(concreteQuery, sqlResult.FetchTree);
            var result = projectionRewriter.Rewrite();
            var projection = result.Mapper(inputs);
            foreach (var assertion in assertions) {
                assertion(projection);
            }
        }

        private SelectWriter GetSql2012Writer(IConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new CustomConfig();
            }

            return new SelectWriter(new SqlServer2012Dialect(), configuration);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new()
        {
            return new SelectQuery<T>(new Mock<IProjectedSelectQueryExecutor>().Object);
        }

        private class CustomConfig : MockConfiguration
        {
            public CustomConfig()
            {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}