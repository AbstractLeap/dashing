namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Extensions;
    using Dashing.Tests.CodeGeneration.Fixtures;
    using Dashing.Tests.TestDomain;

    using Xunit;

    public class WhereClauseWriterTests : IUseFixture<GenerateCodeFixture> {
        private IGeneratedCodeManager codeManager;

        public void SetFixture(GenerateCodeFixture data) {
            this.codeManager = data.CodeManager;
        }

        [Fact]
        public void PreviousForeignKeyPredicateDoesntInterrupt() {
            var target = MakeTarget();
            var author = new User { UserId = 1 };
            Expression<Func<Comment, bool>> pred = c => c.Post.Author.UserId == author.UserId && c.Post.Author.IsEnabled;
            var result = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where ((t_100.[AuthorId] = @l_1) and t_101.[IsEnabled] = 1)", result.Sql);
        }

        [Fact]
        public void UnaryBoolGetsEqualsOne() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo;
            var result = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where [IsFoo] = 1", result.Sql);
        }

        [Fact]
        public void BinaryBoolDoesNotGetExtraOne() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo == true;
            var result = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where ([IsFoo] = @l_1)", result.Sql);
        }

        [Fact]
        public void BinaryBoolClosureGetsParameter() {
            var target = MakeTarget();
            var boolClass = new BoolClass { IsFoo = true };
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo == boolClass.IsFoo;
            var result = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where ([IsFoo] = @l_1)", result.Sql);
        }

        [Fact]
        public void NegatedUnaryBoolGetsNotEqualOne() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => !b.IsFoo;
            var result = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where [IsFoo] = 0", result.Sql);
        }

        [Fact]
        public void UnaryWithinBinaryWorks() {
            var target = MakeTarget();
            Expression<Func<BoolClass, bool>> pred = b => b.IsFoo && b.BoolClassId == 1;
            var result = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where ([IsFoo] = 1 and ([BoolClassId] = @l_1))", result.Sql);
        }

        [Fact]
        public void StaticPropertyMakesParameter() {
            var start = DateTime.UtcNow;
            var target = MakeTarget();
            Expression<Func<Comment, bool>> pred = c => c.CommentDate < DateTime.UtcNow;
            var result = target.GenerateSql(new[] { pred }, null);
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
            var result = target.GenerateSql(new List<Expression<Func<Post, bool>>> { whereClause1,  whereClause2 }, null);

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
            var result = target.GenerateSql(new List<Expression<Func<Post, bool>>> { whereClause1, whereClause2 }, null);

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
            var actual = target.GenerateSql(new[] { whereClause }, null);

            // assert
            Assert.Equal(" where ([UserId] = @l_1)", actual.Sql);
        }

        [Fact]
        public void WhereEntityEqualsTrackedEntity() {
            // assemble
            var target = MakeTarget();
            var post = this.codeManager.CreateTrackingInstance<Post>();
            post.PostId = 1;
            this.codeManager.TrackInstance(post);
            Expression<Func<Post, bool>> whereClause = p => p == post;

            // act
            var actual = target.GenerateSql(new[] { whereClause }, null);

            // assert
            Assert.Equal(" where ([PostId] = @l_1)", actual.Sql);
            Assert.Equal(typeof(int), actual.Parameters.GetValue("l_1").GetType());
        }

        [Fact]
        public void WhereEntityEqualsGeneratedEntity() {
            // assemble
            var target = MakeTarget();
            var post = this.codeManager.CreateForeignKeyInstance<Post>();
            post.PostId = 1;
            Expression<Func<Post, bool>> whereClause = p => p == post;

            // act
            var actual = target.GenerateSql(new[] { whereClause }, null);

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
                return this.writer.GenerateSql(this.whereClauses, null);
            }
        }

        private static class WhereOnInterfaceDemonstrator<T> where T : IEnableable {
            public static void ActUpon(WhereClauseWriterHarness<T> harness) {
                harness.Where(t => t.IsEnabled);
            }
        }

        private static class WhereOnGenericTypeConstraintDemonstrator<T> where T : class, IEnableable {
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
            var actual = target.GenerateSql(new[] {pred}, null);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringContainsParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.Contains("Foo");
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal("%Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringStartsWith() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.StartsWith("Foo");
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringStartsWithParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.StartsWith("Foo");
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal("Foo%", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereStringEndsWith() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.EndsWith("Foo");
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where [Title] like @l_1", actual.Sql);
        }

        [Fact]
        public void WhereStringEndsWithParamsGood() {
            var target = MakeTarget();
            Expression<Func<Post, bool>> pred = p => p.Title.EndsWith("Foo");
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal("%Foo", actual.Parameters.GetValue("l_1"));
        }

        [Fact]
        public void WhereContainsOnQueryable() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Expression<Func<Post, bool>> pred = p => ints.Where(i => i % 2 == 0).Contains(p.PostId);
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(" where [PostId] in @l_1", actual.Sql);
        }

        [Fact]
        public void WhereContainsOnQueryableGetGoodParam() {
            var target = MakeTarget();
            var ints = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Expression<Func<Post, bool>> pred = p => ints.Where(i => i % 2 == 0).Contains(p.PostId);
            var actual = target.GenerateSql(new[] { pred }, null);
            Assert.Equal(new[] {2, 4, 6, 8 }, actual.Parameters.GetValue("l_1") as IEnumerable<int>);
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