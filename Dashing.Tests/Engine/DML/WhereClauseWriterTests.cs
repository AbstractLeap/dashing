namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Extensions;
    using Dashing.Tests.TestDomain;

    using Xunit;
    using Xunit.Abstractions;

    public class WhereClauseWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public WhereClauseWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void NullLeftHandSideGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => null == c.Content;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ([Content] is @l_1)", result.Sql);
        }

        [Fact]
        public void NullLeftHandSideGetsGoodParams() {
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => null == c.Content;
            var result = GenerateSql(target, pred);
            Assert.Null(result.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void NullValueGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => c.Content == null;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ([Content] is null)", result.Sql);
        }

        [Fact]
        public void NullValueGetsGoodParams() {
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => c.Content == null;
            var result = GenerateSql(target, pred);
            Assert.Empty(result.Parameters.ParameterNames);
        }

        [Fact]
        public void NotNullValueGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => c.Content != null;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ([Content] is not null)", result.Sql);
        }

        [Fact]
        public void NotNullValueGetsGoodParams() {
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => c.Content != null;
            var result = GenerateSql(target, pred);
            Assert.Empty(result.Parameters.ParameterNames);
        }

        [Fact]
        public void NullConstantGetsGoodSql() {
            var target = MakeTarget();
            var c1 = new Comment();
            Expression<Func<Comment, bool>> pred = c => c.Content == c1.Content;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ([Content] is null)", result.Sql);
        }

        [Fact]
        public void NullConstantGetsGoodParams() {
            var target = MakeTarget();
            var c1 = new Comment();
            Expression<Func<Comment, bool>> pred = c => c.Content == c1.Content;
            var result = GenerateSql(target, pred);
            Assert.Empty(result.Parameters.ParameterNames);
        }

        [Fact]
        public void PreviousForeignKeyPredicateDoesntInterrupt() {
            var target = MakeTarget();
            var author = new User { UserId = 1 };
            Expression<Func<Comment, bool>> pred = c => c.Post.Author.UserId == author.UserId && c.Post.Author.IsEnabled;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ((t_100.[AuthorId] = @l_1) and t_101.[IsEnabled] = 1)", result.Sql);
        }

        [Fact]
        public void PreviousForeignKeyPredicateDoesntInterruptOnDifferentBranch() {
            var target = MakeTarget();
            var author = new User { UserId = 1 };
            Expression<Func<Comment, bool>> pred = c => c.Post.Author.UserId == author.UserId && c.User.IsEnabled;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ((t_100.[AuthorId] = @l_1) and t_101.[IsEnabled] = 1)", result.Sql);
        }

        [Fact]
        public void UnaryBoolGetsEqualsOne() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where [IsFoo] = 1", result.Sql);
        }

        [Fact]
        public void BinaryBoolDoesNotGetExtraOne() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where [IsFoo] = 1", result.Sql);
        }

        [Fact]
        public void BinaryBoolClosureGetsParameter() {
            var target = MakeTarget();
            var boolClass = new BoolClass { IsFoo = true };
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo == boolClass.IsFoo;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ([IsFoo] = @l_1)", result.Sql);
        }

        [Fact]
        public void NegatedUnaryBoolGetsNotEqualOne() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => !b.IsFoo;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where [IsFoo] = 0", result.Sql);
        }

        [Fact]
        public void UnaryWithinBinaryWorks() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo && b.BoolClassId == 1;
            var result = GenerateSql(target, pred);
            Assert.Equal(" where ([IsFoo] = 1 and ([BoolClassId] = @l_1))", result.Sql);
        }

        [Fact]
        public void NotBracketsWorks() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !(p.Author.EmailAddress == "Foo" && p.Rating > 3);
            var result = GenerateSql(target, pred);
            Assert.Equal(" where not ((t_100.[EmailAddress] = @l_1) and (t.[Rating] > @l_2))", result.Sql);
        }

        [Fact]
        public void NotSingleBracketWorks() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !(p.Rating > 3);
            var result = GenerateSql(target, pred);
            Assert.Equal(" where not ([Rating] > @l_1)", result.Sql);
        }

        [Fact]
        public void StaticPropertyMakesParameter() {
            var start = DateTime.UtcNow;
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => c.CommentDate < DateTime.UtcNow;
            var result = GenerateSql(target, pred);
            var end = DateTime.UtcNow;
            var param = (DateTime)result.Parameters.GetValue("l_1");
            Assert.Equal(" where ([CommentDate] < @l_1)", result.Sql);
            Assert.True(param >= start && param <= end);
        }

        [Fact]
        public void TwoWhereClausesStack() {
            // assemble
            var target = MakeTarget();
            Expression<Func<Post, bool>> whereClause1 = p => p.PostId > 0;
            Expression<Func<Post, bool>> whereClause2 = p => p.PostId < 2;

            // act
            var result = GenerateSql(target, new List<Expression<Func<Post, bool>>> { whereClause1, whereClause2 });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal(" where ([PostId] > @l_1) and ([PostId] < @l_2)", result.Sql);
        }

        [Fact]
        public void TwoWhereClausesParametersOkay() {
            // assemble
            var target = MakeTarget();
            Expression<Func<Post, bool>> whereClause1 = p => p.PostId > 0;
            Expression<Func<Post, bool>> whereClause2 = p => p.PostId < 2;

            // act
            var result = GenerateSql(target, new List<Expression<Func<Post, bool>>> { whereClause1, whereClause2 });

            // assert
            Debug.Write(result.Sql);
            Assert.Equal(0, result.Parameters.GetValue("l_1"));
            Assert.Equal(2, result.Parameters.GetValue("l_2"));
        }

        [Fact]
        public void UsesPrimaryKeyWhereEntityEqualsEntity() {
            // assemble
            var target = MakeTarget();
            var user = new User { UserId = 1 };
            Expression<Func<User, bool>> whereClause = u => u == user;

            // act
            var actual = GenerateSql(target, whereClause);

            // assert
            Assert.Equal(" where ([UserId] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereEntityEqualsTrackedEntity() {
            // assemble
            var target = MakeTarget();
            var post = new Post();
            post.PostId = 1;
            ((ITrackedEntity)post).EnableTracking();
            Expression<Func<Post, bool>> whereClause = p => p == post;

            // act
            var actual = GenerateSql(target, whereClause);

            // assert
            Assert.Equal(" where ([PostId] = @l_1)", actual.Sql);
            Assert.Equal(typeof(int), actual.Parameters.GetValue("l_1").GetType());
        }

        [Fact]
        public void WhereEntityEqualsGeneratedEntity() {
            // assemble
            var target = MakeTarget();
            var post = new Post();
            post.PostId = 1;
            Expression<Func<Post, bool>> whereClause = p => p == post;

            // act
            var actual = GenerateSql(target, whereClause);

            // assert
            Assert.Equal(" where ([PostId] = @l_1)", actual.Sql);
            Assert.Equal(typeof(int), actual.Parameters.GetValue("l_1").GetType());
        }

        private class WhereClauseWriterHarness<T> {
            private readonly WhereClauseWriter writer;

            private readonly IList<Expression<Func<T, bool>>> whereClauses;

            public WhereClauseWriterHarness(WhereClauseWriter writer) {
                this.writer = writer;
                this.whereClauses = new List<Expression<Func<T, bool>>>();
            }

            public void Where(Expression<Func<T, bool>> predicate) {
                this.whereClauses.Add(predicate);
            }

            public SelectWriterResult Execute() {
                return this.writer.GenerateSql(this.whereClauses, null, new AutoNamingDynamicParameters(), new DefaultAliasProvider());
            }
        }

        private static class WhereOnInterfaceDemonstrator<T>
            where T : IEnableable {
            public static void ActUpon(WhereClauseWriterHarness<T> harness) {
                harness.Where(t => t.IsEnabled);
            }
        }

        private static class WhereOnGenericTypeConstraintDemonstrator<T>
            where T : class, IEnableable {
            public static void ActUpon(WhereClauseWriterHarness<T> harness) {
                harness.Where(t => t.IsEnabled);
            }
        }

        [Fact]
        public void WhereOnInterface() {
            // assemble
            var target = MakeTarget();

            // act
            var harness = new WhereClauseWriterHarness<User>(target);
            WhereOnInterfaceDemonstrator<User>.ActUpon(harness);
            var actual = harness.Execute();

            // assert
            Assert.Equal(" where [IsEnabled] = 1", actual.Sql);
        }

        [Fact]
        public void WhereOnGenericTypeConstraint() {
            // assemble
            var target = MakeTarget();

            // act
            var harness = new WhereClauseWriterHarness<User>(target);
            WhereOnGenericTypeConstraintDemonstrator<User>.ActUpon(harness);
            var actual = harness.Execute();

            // assert
            Assert.Equal(" where [IsEnabled] = 1", actual.Sql);
        }

        [Fact]
        public void WhereStringContains() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.Contains("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringContainsUsingClosure() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => p.Title.Contains(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringContainsUsingClosureGetsGoodParam() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => p.Title.Contains(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringStartsWithUsingClosure() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => p.Title.StartsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringStartsWithUsingClosureGetsGoodParam() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => p.Title.StartsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal("Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringEndsWithUsingClosure() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => p.Title.EndsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringEndsWithUsingClosureGetsGoodParam() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => p.Title.EndsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringContainsParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.Contains("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringStartsWith() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.StartsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringStartsWithParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.StartsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal("Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringEndsWith() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.EndsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringEndsWithParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.EndsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereContainsEntity() {
            var target = MakeTarget();
            var blogs = new[] { new Blog { BlogId = 1 }, new Blog { BlogId = 2 } };
            Expression<Func<Blog, bool>> pred = b => blogs.Contains(b);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [BlogId] in @l_1", actual.Sql);
            Assert.Equal(new[] { 1, 2 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }
		
		[Fact]
        public void WhereContainsEntityWithoutClosure() {
            var target = MakeTarget();
            var blogs = new[] { new Blog { BlogId = 1 }, new Blog { BlogId = 2 } };
            var param = Expression.Parameter(typeof(Blog), "b");
            var body = Expression.Call(
                typeof(Enumerable).GetMethods().Single(m => m.Name == "Contains" && m.GetParameters().Length == 2).MakeGenericMethod(typeof(Blog)),
                Expression.Constant(blogs),
                param);
            Expression<Func<Blog, bool>> pred = Expression.Lambda<Func<Blog, bool>>(body, param);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [BlogId] in @l_1", actual.Sql);
            Assert.Equal(new[] { 1, 2 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereContainsRelatedEntity() {
            var target = MakeTarget();
            var blogs = new[] { new Blog { BlogId = 1 }, new Blog { BlogId = 2 } };
            Expression<Func<Post, bool>> pred = p => blogs.Contains(p.Blog);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [BlogId] in @l_1", actual.Sql);
            Assert.Equal(new[] { 1, 2 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereNotContainsEntity() {
            var target = MakeTarget();
            var blogs = new[] { new Blog { BlogId = 1 }, new Blog { BlogId = 2 } };
            Expression<Func<Blog, bool>> pred = b => !blogs.Contains(b);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [BlogId] not in @l_1", actual.Sql);
            Assert.Equal(new[] { 1, 2 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereNotContainsRelatedEntity() {
            var target = MakeTarget();
            var blogs = new[] { new Blog { BlogId = 1 }, new Blog { BlogId = 2 } };
            Expression<Func<Post, bool>> pred = p => !blogs.Contains(p.Blog);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [BlogId] not in @l_1", actual.Sql);
            Assert.Equal(new[] { 1, 2 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereContainsString() {
            var target = MakeTarget();
            var values = new[] { "Foo", "Bar" };
            Expression<Func<Post, bool>> pred = p => values.Contains(p.Title);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] in @l_1", actual.Sql);
            Assert.Equal(new[] { "Foo", "Bar" }, actual.Parameters.GetValue("l_1") as IEnumerable<string>);
        }

        private static readonly string[] WhereContainsStringStaticValues = new[] { "Foo", "Bar" };

        [Fact]
        public void WhereContainsStringStatic() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => WhereContainsStringStaticValues.Contains(p.Title);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] in @l_1", actual.Sql);
            Assert.Equal(new[] { "Foo", "Bar" }, actual.Parameters.GetValue("l_1") as IEnumerable<string>);
        }

        [Fact]
        public void WhereContainsOnQueryable() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Expression<Func<Post, bool>> pred = p => ints.Where(i => i % 2 == 0).Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [PostId] in @l_1", actual.Sql);
        }

        [Fact]
        public void WhereContainsOnQueryableGetGoodParam() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Expression<Func<Post, bool>> pred = p => ints.Where(i => i % 2 == 0).Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(new[] { 2, 4, 6, 8 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereClauseOnHashsetGetsGoodQuery() {
            var target = MakeTarget();
            var ints = new HashSet<int>(new[] { 1, 2, 3, 4, 5 });
            Expression<Func<Post, bool>> pred = p => ints.Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [PostId] in @l_1", actual.Sql);
        }

        [Fact]
        public void WhereClauseOnHashsetGetsGoodParams() {
            var target = MakeTarget();
            var ints = new HashSet<int>(new[] { 1, 2, 3, 4, 5 });
            Expression<Func<Post, bool>> pred = p => ints.Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereNotContainsQueryableGetsGoodSql() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Expression<Func<Post, bool>> pred = p => !ints.Where(i => i % 2 == 0).Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [PostId] not in @l_1", actual.Sql);
        }

        [Fact]
        public void WhereNotContainsQueryableGetsGoodParams() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Expression<Func<Post, bool>> pred = p => !ints.Where(i => i % 2 == 0).Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(new[] { 2, 4, 6, 8 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereNotContainsOnHashsetGetsGoodQuery() {
            var target = MakeTarget();
            var ints = new HashSet<int>(new[] { 1, 2, 3, 4, 5 });
            Expression<Func<Post, bool>> pred = p => !ints.Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [PostId] not in @l_1", actual.Sql);
        }

        [Fact]
        public void WhereNotContainsOnHashsetGetsGoodParams() {
            var target = MakeTarget();
            var ints = new HashSet<int>(new[] { 1, 2, 3, 4, 5 });
            Expression<Func<Post, bool>> pred = p => !ints.Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereNotContainsGetsGoodQuery() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5 };
            Expression<Func<Post, bool>> pred = p => !ints.Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [PostId] not in @l_1", actual.Sql);
        }

        [Fact]
        public void WhereNotContainsGetsGoodParams() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5 };
            Expression<Func<Post, bool>> pred = p => !ints.Contains(p.PostId);
            var actual = GenerateSql(target, pred);
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
        }

        [Fact]
        public void WhereStringNotContains() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Title.Contains("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] not like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringNotContainsUsingClosure() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => !p.Title.Contains(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] not like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringNotContainsUsingClosureGetsGoodParam() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => !p.Title.Contains(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringNotStartsWithUsingClosure() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => !p.Title.StartsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] not like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringNotStartsWithUsingClosureGetsGoodParam() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => !p.Title.StartsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal("Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringNotEndsWithUsingClosure() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => !p.Title.EndsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] not like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringNotEndsWithUsingClosureGetsGoodParam() {
            var target = MakeTarget();
            var c = new Comment { Content = "Foo" };
            Expression<Func<Post, bool>> pred = p => !p.Title.EndsWith(c.Content);
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringNotContainsParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Title.Contains("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringNotStartsWith() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Title.StartsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] not like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringNotStartsWithParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Title.StartsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal("Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringNotEndsWith() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Title.EndsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where [Title] not like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringNotEndsWithParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Title.EndsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal("%Foo", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereAnyGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Comments.Any(c => c.Content == "foo");
            var actual = GenerateSql(target, pred);
            this.outputHelper.WriteLine(actual.Sql);
            Assert.Equal(" where exists (select 1 from [Comments] as i where (i.[Content] = @l_1) and t.[PostId] = i.[PostId])", actual.Sql);
        }

        [Fact]
        public void WhereNotAnyGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => !p.Comments.Any(c => c.Content == "foo");
            var actual = GenerateSql(target, pred);
            this.outputHelper.WriteLine(actual.Sql);
            Assert.Equal(" where not exists (select 1 from [Comments] as i where (i.[Content] = @l_1) and t.[PostId] = i.[PostId])", actual.Sql);
        }

        [Fact]
        public void WhereAnyRelatedGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Comments.Any(c => c.User.EmailAddress == "foo");
            var actual = GenerateSql(target, pred);
            this.outputHelper.WriteLine(actual.Sql);
            Assert.Equal(" where exists (select 1 from [Comments] as i inner join [Users] as i_100 on i.UserId = i_100.UserId where (i_100.[EmailAddress] = @l_1) and t.[PostId] = i.[PostId])", actual.Sql);
        }

        [Fact]
        public void WhereAnyAndGetsGoodSql() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Comments.Any(c => c.Content == "foo" && c.CommentDate > DateTime.UtcNow);
            var actual = GenerateSql(target, pred);
            this.outputHelper.WriteLine(actual.Sql);
            Assert.Equal(" where exists (select 1 from [Comments] as i where ((i.[Content] = @l_1) and (i.[CommentDate] > @l_2)) and t.[PostId] = i.[PostId])", actual.Sql);
        }

        [Fact]
        public void WhereDictionaryItemGetsGoodSql() {
            var target = MakeTarget();
            var dict = new Dictionary<string, string> { { "Foo", "Bar" } };
            Expression<Func<Post, bool>> pred = p => p.Content == dict["Foo"];
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([Content] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereDictionaryItemGetsGoodParams() {
            var target = MakeTarget();
            var dict = new Dictionary<string, string> { { "Foo", "Bar" } };
            Expression<Func<Post, bool>> pred = p => p.Content == dict["Foo"];
            var actual = GenerateSql(target, pred);
            Assert.Equal("Bar", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereNestedDictionaryItemGetsGoodParams() {
            var target = MakeTarget();
            var dict = new { A = new Dictionary<string, string> { { "Foo", "Bar" } } };
            Expression<Func<Post, bool>> pred = p => p.Content == dict.A["Foo"];
            var actual = GenerateSql(target, pred);
            Assert.Equal("Bar", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereDictionaryItemWithPropertyGetsGoodParams() {
            var target = MakeTarget();
            var dict = new Dictionary<string, Post> { { "Foo", new Post { Content = "Bar" } } };
            Expression<Func<Post, bool>> pred = p => p.Content == dict["Foo"].Content;
            var actual = GenerateSql(target, pred);
            Assert.Equal("Bar", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereEqualsEntityWorks() {
            var target = MakeTarget();
            var blog = new Blog { BlogId = 1 };
            Expression<Func<Post, bool>> pred = p => p.Blog.Equals(blog);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([BlogId] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereEqualsIntWorks() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.PostId.Equals(1);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([PostId] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereEqualsStringWorks() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.Equals("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([Title] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereEqualsAcrossJoinWorks() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Blog.CreateDate.Equals(DateTime.UtcNow);
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where (t_100.[CreateDate] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereHasNullableHasValue() {
            var target = MakeTarget();
            Expression<Func<ThingWithNullable, bool>> pred = p => p.Nullable.HasValue;
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([Nullable] is not null)", actual.Sql);
        }

        [Fact]
        public void WhereHasNullableDoesNotHasValue() {
            var target = MakeTarget();
            Expression<Func<ThingWithNullable, bool>> pred = p => !p.Nullable.HasValue;
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([Nullable] is null)", actual.Sql);
        }

        [Fact]
        public void WhereEqualsNullable() {
            var target = MakeTarget();
            var val = new int?(1);
            Expression<Func<ThingWithNullable, bool>> pred = p => p.Nullable == val.Value;
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ([Nullable] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereContainsNullableCheck() {
            var target = MakeTarget();
            Expression<Func<ReferencesThingWithNullable, bool>> pred = p => p.Thing.Nullable.HasValue && p.Thing.Name.StartsWith("Foo");
            var actual = GenerateSql(target, pred);
            Assert.Equal(" where ((t_100.[Nullable] is not null) and t_100.[Name] like @l_1)", actual.Sql);
        }

        private static SelectWriterResult GenerateSql<T>(WhereClauseWriter target, Expression<Func<T, bool>> whereClause, QueryTree queryTree = null) {
            return GenerateSql(target, new[] { whereClause }, queryTree);
        }

        private static SelectWriterResult GenerateSql<T>(WhereClauseWriter target, IEnumerable<Expression<Func<T, bool>>> whereClauses, QueryTree queryTree = null) {
            return target.GenerateSql(whereClauses, queryTree, new AutoNamingDynamicParameters(), new DefaultAliasProvider());
        }

        private static WhereClauseWriter MakeTarget() {
            return new WhereClauseWriter(new SqlServerDialect(), MakeConfig());
        }

        private static IConfiguration MakeConfig() {
            return new CustomConfig();
        }

        private class CustomConfig : MockConfiguration {
            public CustomConfig() {
                this.AddNamespaceOf<Post>();
            }
        }
    }
}