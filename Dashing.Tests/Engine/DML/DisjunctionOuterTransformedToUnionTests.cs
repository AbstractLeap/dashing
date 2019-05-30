namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.Engine.InMemory;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class DisjunctionOuterTransformedToUnionTests {
        private readonly ITestOutputHelper outputHelper;

        public DisjunctionOuterTransformedToUnionTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void NoDisjunctionsReturnsSame() {
            Expression<Func<Post, bool>> pred = p => p.Author.UserId == 1;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void RootUnaryDisjunctionReturnsSame() {
            Expression<Func<BoolClass, bool>> pred = p => p.IsFoo || p.BoolClassId == 1;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void NonRootJoinDisjunctionWorks() {
            Expression<Func<Post, bool>> pred = p => p.Author.Username == "Mark" || p.Blog.Title == "Foo";
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(2, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Author.Username == "Mark")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Blog.Title == "Foo")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(1)
                      .ToDebugString());
        }

        [Fact]
        public void NonRootBoolMemberAccessJoinDisjunctionWorks() {
            var user = new User {
                                    UserId = 1
                                };
            Expression<Func<Comment, bool>> pred = p => p.Post.DoNotMap || p.User == user;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(2, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<Comment, bool>>)(p => p.Post.DoNotMap)).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Comment, bool>>)(p => p.User == user)).ToDebugString(),
                result.UnionWhereClauses.ElementAt(1)
                      .ToDebugString());
        }

        [Fact]
        public void NonRootNotBoolUnaryJoinDisjunctionWorks() {
            var user = new User {
                                    UserId = 1
                                };
            Expression<Func<Comment, bool>> pred = p => !p.Post.DoNotMap || p.User == user;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(2, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<Comment, bool>>)(p => !p.Post.DoNotMap)).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Comment, bool>>)(p => p.User == user)).ToDebugString(),
                result.UnionWhereClauses.ElementAt(1)
                      .ToDebugString());
        }

        [Fact]
        public void SingleNonRootJoinDisjunctionWorks() {
            Expression<Func<Post, bool>> pred = p => p.Author.Username == "Mark" || p.Title == "Foo";
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(2, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Author.Username == "Mark")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Title == "Foo")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(1)
                      .ToDebugString());
        }

        [Fact]
        public void SingleNonRootTripleDisjunctionWorks() {
            Expression<Func<Post, bool>> pred = p => p.Author.Username == "Mark" || p.Title == "Foo" || p.Blog.Title == "Bar";
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(3, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Author.Username == "Mark")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Title == "Foo")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(1)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Blog.Title == "Bar")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(2)
                      .ToDebugString());
        }

        [Fact]
        public void OrAndAndDisjunctionWorks() {
            Expression<Func<Post, bool>> pred = p => p.Rating > 5 && (p.Author.Username == "Mark" || p.Blog.Title == "Bar");
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(2, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Rating > 5 && p.Author.Username == "Mark")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<Post, bool>>)(p => p.Rating > 5 && p.Blog.Title == "Bar")).ToDebugString(),
                result.UnionWhereClauses.ElementAt(1)
                      .ToDebugString());
        }

        [Fact]
        public void MultipleOrDisjunctionDoesnt() {
            Expression<Func<Post, bool>> pred = p => (p.Rating > 5 && (p.Author.Username == "Mark" || p.Blog.Title == "Bar")) || (p.Rating < 5 && (p.Author.Username == "Tom" || p.Blog.Title == "Car"));
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void RootJoinDisjunctionDoesnt() {
            Expression<Func<Post, bool>> pred = p => p.Title == "Foo" || p.Rating == 5;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void RootForeignKeyJoinDisjunctionDoesnt() {
            Expression<Func<Post, bool>> pred = p => p.Author.UserId == 1 || p.Blog.BlogId == 2;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void RootForeignKeyEntityJoinDisjunctionDoesnt() {
            var author = new User {
                                      UserId = 1
                                  };
            var blog = new Blog {
                                    BlogId = 2
                                };
            Expression<Func<Post, bool>> pred = p => p.Author == author || p.Blog == blog;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void NoDisjunctionsReturnsSameSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.UserId == 1) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where ([AuthorId] = @l_1)", result.Sql);
        }

        [Fact]
        public void RootUnaryDisjunctionReturnsSameSql() {
            var query = GetSelectQuery<BoolClass>()
                            .Where(p => p.IsFoo || p.BoolClassId == 1) as SelectQuery<BoolClass>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select [BoolClassId], [IsFoo] from [BoolClasses] where ([IsFoo] = 1 or ([BoolClassId] = @l_1))", result.Sql);
        }

        [Fact]
        public void NonRootJoinDisjunctionWorksSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.Username == "Mark" || p.Blog.Title == "Foo") as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where (t_100.[Username] = @l_1) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] = @l_2)", result.Sql);
        }

        [Fact]
        public void NonRootBoolMemberAccessJoinDisjunctionWorksSql() {
            var user = new User {
                                    UserId = 1
                                };
            var query = GetSelectQuery<Comment>()
                            .Where(p => p.Post.DoNotMap || p.User == user) as SelectQuery<Comment>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[CommentId], t.[Content], t.[PostId], t.[UserId], t.[CommentDate] from [Comments] as t inner join [Posts] as t_100 on t.PostId = t_100.PostId where t_100.[DoNotMap] = 1 union select t.[CommentId], t.[Content], t.[PostId], t.[UserId], t.[CommentDate] from [Comments] as t where (t.[UserId] = @l_1)", result.Sql);
        }

        [Fact]
        public void NonRootNotBoolUnaryJoinDisjunctionWorksSql() {
            var user = new User {
                                    UserId = 1
                                };
            var query = GetSelectQuery<Comment>()
                            .Where(p => !p.Post.DoNotMap || p.User == user) as SelectQuery<Comment>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[CommentId], t.[Content], t.[PostId], t.[UserId], t.[CommentDate] from [Comments] as t inner join [Posts] as t_100 on t.PostId = t_100.PostId where t_100.[DoNotMap] = 0 union select t.[CommentId], t.[Content], t.[PostId], t.[UserId], t.[CommentDate] from [Comments] as t where (t.[UserId] = @l_1)", result.Sql);
        }

        [Fact]
        public void SingleNonRootJoinDisjunctionWorksSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.Username == "Mark" || p.Title == "Foo") as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where (t_100.[Username] = @l_1) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t where (t.[Title] = @l_2)", result.Sql);
        }

        [Fact]
        public void SingleNonRootTripleDisjunctionWorksSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.Username == "Mark" || p.Title == "Foo" || p.Blog.Title == "Bar") as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where (t_100.[Username] = @l_1) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t where (t.[Title] = @l_2) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] = @l_3)", result.Sql);
        }

        [Fact]
        public void OrAndAndDisjunctionWorksSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Rating > 5 && (p.Author.Username == "Mark" || p.Blog.Title == "Bar")) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where ((t.[Rating] > @l_1) and (t_100.[Username] = @l_2)) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where ((t.[Rating] > @l_3) and (t_100.[Title] = @l_4))", result.Sql);
        }

        [Fact]
        public void MultipleOrDisjunctionDoesntSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => (p.Rating > 5 && (p.Author.Username == "Mark" || p.Blog.Title == "Bar")) || (p.Rating < 5 && (p.Author.Username == "Tom" || p.Blog.Title == "Car"))) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t left join [Users] as t_100 on t.AuthorId = t_100.UserId left join [Blogs] as t_101 on t.BlogId = t_101.BlogId where (((t.[Rating] > @l_1) and ((t_100.[Username] = @l_2) or (t_101.[Title] = @l_3))) or ((t.[Rating] < @l_4) and ((t_100.[Username] = @l_5) or (t_101.[Title] = @l_6))))", result.Sql);
        }

        [Fact]
        public void RootJoinDisjunctionDoesntSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Title == "Foo" || p.Rating == 5) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where (([Title] = @l_1) or ([Rating] = @l_2))", result.Sql);
        }

        [Fact]
        public void RootForeignKeyJoinDisjunctionDoesntSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.UserId == 1 || p.Blog.BlogId == 2) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where (([AuthorId] = @l_1) or ([BlogId] = @l_2))", result.Sql);
        }

        [Fact]
        public void RootForeignKeyEntityJoinDisjunctionDoesntSql() {
            var author = new User {
                                      UserId = 1
                                  };
            var blog = new Blog {
                                    BlogId = 2
                                };
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author == author || p.Blog == blog) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select [PostId], [Title], [Content], [Rating], [AuthorId], [BlogId], [DoNotMap] from [Posts] where (([AuthorId] = @l_1) or ([BlogId] = @l_2))", result.Sql);
        }

        [Fact]
        public void SingleDisjunctionInMultipleWhereClausesWorks() {
            var query = GetSelectQuery<Post>()
                        .Where(p => p.Blog.Title == "Foo" || p.Author.HeightInMeters == 2)
                        .Where(p => p.Rating > 5) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] = @l_1) and (t.[Rating] > @l_2) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where (t_100.[HeightInMeters] = @l_3) and (t.[Rating] > @l_4)", result.Sql);
        }

        private SelectWriter GetSql2012Writer(IConfiguration configuration = null) {
            if (configuration == null) {
                configuration = new CustomConfig();
            }

            return new SelectWriter(new SqlServer2012Dialect(), configuration);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new() {
            return new SelectQuery<T>(new Mock<ISelectQueryExecutor>().Object);
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}