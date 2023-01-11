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
        public void NullableRootThingWorks() {
            Expression<Func<ThingWithNullable, bool>> pred = p => !p.Nullable.HasValue || p.Nullable > 3;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.False(result.ContainsOuterJoinDisjunction);
        }

        [Fact]
        public void NonRootNullableJoinDisjunctionWorks() {
            Expression<Func<ReferencesThingWithNullable, bool>> pred = p => !p.Thing.Nullable.HasValue || p.Thing.Nullable > 3;
            var outerJoinDisjunctionTransformer = new OuterJoinDisjunctionTransformer(new CustomConfig());
            var result = outerJoinDisjunctionTransformer.AttemptGetOuterJoinDisjunctions(pred);
            Assert.True(result.ContainsOuterJoinDisjunction);
            Assert.Equal(2, result.UnionWhereClauses.Count());
            Assert.Equal(
                ((Expression<Func<ReferencesThingWithNullable, bool>>)(p => !p.Thing.Nullable.HasValue)).ToDebugString(),
                result.UnionWhereClauses.ElementAt(0)
                      .ToDebugString());
            Assert.Equal(
                ((Expression<Func<ReferencesThingWithNullable, bool>>)(p => p.Thing.Nullable > 3)).ToDebugString(),
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
        public void AnyDoesntParticipateInDisjunction()
        {
            Expression<Func<Blog, bool>> pred = b => b.Posts.Any(p => p.Author.Username == "bob" || p.Author.Username == "Dave");
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
            Assert.Equal("select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t where (t.[AuthorId] = @l_1)", result.Sql);
        }

        [Fact]
        public void RootUnaryDisjunctionReturnsSameSql() {
            var query = GetSelectQuery<BoolClass>()
                            .Where(p => p.IsFoo || p.BoolClassId == 1) as SelectQuery<BoolClass>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal("select t.[BoolClassId], t.[IsFoo] from [BoolClasses] as t where (t.[IsFoo] = 1 or (t.[BoolClassId] = @l_1))", result.Sql);
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
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t where ((t.[Title] = @l_1) or (t.[Rating] = @l_2))", result.Sql);
        }

        [Fact]
        public void RootForeignKeyJoinDisjunctionDoesntSql() {
            var query = GetSelectQuery<Post>()
                            .Where(p => p.Author.UserId == 1 || p.Blog.BlogId == 2) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t where ((t.[AuthorId] = @l_1) or (t.[BlogId] = @l_2))", result.Sql);
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
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t where ((t.[AuthorId] = @l_1) or (t.[BlogId] = @l_2))", result.Sql);
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

        [Fact]
        public void MultipleDisjunctionInMultipleWhereClausesDoesnt() {
            var query = GetSelectQuery<Post>()
                        .Where(p => p.Blog.Title == "Foo" || p.Author.HeightInMeters == 2)
                        .Where(p => p.Rating > 5 || p.Blog.CreateDate > DateTime.UtcNow) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t left join [Users] as t_100 on t.AuthorId = t_100.UserId left join [Blogs] as t_101 on t.BlogId = t_101.BlogId where ((t_101.[Title] = @l_1) or (t_100.[HeightInMeters] = @l_2)) and ((t.[Rating] > @l_3) or (t_101.[CreateDate] > @l_4))", result.Sql);
        }

        [Fact]
        public void OrderByInDisjunctionAppliedLast() {
            var query = GetSelectQuery<Post>()
                        .Where(p => p.Blog.Title == "Foo" || p.Author.HeightInMeters == 2)
                        .OrderBy(p => p.Title) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Blogs] as t_100 on t.BlogId = t_100.BlogId where (t_100.[Title] = @l_1) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[BlogId], t.[DoNotMap] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId where (t_100.[HeightInMeters] = @l_2) order by t.[Title] asc", result.Sql);
        }

        [Fact]
        public void FetchAcrossJoinsDoesNotInferInnerQuery() {
            var query = GetSelectQuery<Post>()
                        .Fetch(p => p.Blog)
                        .Where(p => p.Blog.Title == "Foo" || p.Author.HeightInMeters == 2) as SelectQuery<Post>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[DoNotMap], t_1.[BlogId], t_1.[Title], t_1.[CreateDate], t_1.[Description], t_1.[OwnerId] from [Posts] as t inner join [Blogs] as t_1 on t.BlogId = t_1.BlogId where (t_1.[Title] = @l_1) union select t.[PostId], t.[Title], t.[Content], t.[Rating], t.[AuthorId], t.[DoNotMap], t_1.[BlogId], t_1.[Title], t_1.[CreateDate], t_1.[Description], t_1.[OwnerId] from [Posts] as t inner join [Users] as t_100 on t.AuthorId = t_100.UserId left join [Blogs] as t_1 on t.BlogId = t_1.BlogId where (t_100.[HeightInMeters] = @l_2)", result.Sql);
        }

        [Fact]
        public void MultipleFetchAcrossJoinsDoesNotInferInnerQuery() {
            var query = GetSelectQuery<Comment>()
                        .Fetch(p => p.Post)
                        .Where(p => p.Post.Blog.Title == "Foo" && (p.User.HeightInMeters > 100 || p.User.EmailAddress == "foo")) as SelectQuery<Comment>;
            var result = this.GetSql2012Writer()
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(result.Sql);
            Assert.Equal(@"select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from [Comments] as t inner join [Posts] as t_1 on t.PostId = t_1.PostId inner join [Blogs] as t_100 on t_1.BlogId = t_100.BlogId inner join [Users] as t_101 on t.UserId = t_101.UserId where ((t_100.[Title] = @l_1) and (t_101.[HeightInMeters] > @l_2)) union select t.[CommentId], t.[Content], t.[UserId], t.[CommentDate], t_1.[PostId], t_1.[Title], t_1.[Content], t_1.[Rating], t_1.[AuthorId], t_1.[BlogId], t_1.[DoNotMap] from [Comments] as t inner join [Posts] as t_1 on t.PostId = t_1.PostId inner join [Blogs] as t_100 on t_1.BlogId = t_100.BlogId inner join [Users] as t_101 on t.UserId = t_101.UserId where ((t_100.[Title] = @l_3) and (t_101.[EmailAddress] = @l_4))", result.Sql);
        }

        [Fact]
        public void AnyDoesntParticipateInDisjunctionSql()
        {
            var query = GetSelectQuery<Blog>()
                        .Where(b => b.CreateDate > DateTime.Now)
                        .Where(b => b.Owner.IsEnabled)
                            .Where(b => b.Posts.Any(p => p.Author.Username == "bob" || p.Author.Username == "Dave")) as SelectQuery<Blog>;
            var config = new CustomConfig();
            var selectResult = this.GetSql2012Writer(config)
                             .GenerateSql(query, new AutoNamingDynamicParameters());

            this.outputHelper.WriteLine(selectResult.Sql);
            Assert.Equal(@"select t.[BlogId], t.[Title], t.[CreateDate], t.[Description], t.[OwnerId] from [Blogs] as t inner join [Users] as t_100 on t.OwnerId = t_100.UserId where (t.[CreateDate] > @l_1) and t_100.[IsEnabled] = 1 and exists (select 1 from [Posts] as i inner join [Users] as i_100 on i.AuthorId = i_100.UserId where (i_100.[Username] = @l_2)  and t.[BlogId] = i.[BlogId] union select 1 from [Posts] as i inner join [Users] as i_100 on i.AuthorId = i_100.UserId where (i_100.[Username] = @l_3) and t.[BlogId] = i.[BlogId])", selectResult.Sql);
            
            var countResult = new CountWriter(new SqlServer2012Dialect(), config).GenerateCountSql(query);
            this.outputHelper.WriteLine(countResult.Sql);
            Assert.Equal("select count(1) from [Blogs] as t inner join [Users] as t_100 on t.OwnerId = t_100.UserId where (t.[CreateDate] > @l_1) and t_100.[IsEnabled] = 1 and exists (select 1 from [Posts] as i inner join [Users] as i_100 on i.AuthorId = i_100.UserId where (i_100.[Username] = @l_2)  and t.[BlogId] = i.[BlogId] union select 1 from [Posts] as i inner join [Users] as i_100 on i.AuthorId = i_100.UserId where (i_100.[Username] = @l_3) and t.[BlogId] = i.[BlogId])", countResult.Sql);
        }

        private SelectWriter GetSql2012Writer(IConfiguration configuration = null) {
            if (configuration == null) {
                configuration = new CustomConfig();
            }

            return new SelectWriter(new SqlServer2012Dialect(), configuration);
        }

        private SelectQuery<T> GetSelectQuery<T>()
            where T : class, new() {
            return new SelectQuery<T>(new Mock<IProjectedSelectQueryExecutor>().Object);
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}