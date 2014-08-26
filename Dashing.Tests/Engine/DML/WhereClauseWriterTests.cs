namespace Dashing.Tests.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            Assert.Equal(" where [IsEnabled]", actual.Sql);
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
            Assert.Equal(" where [IsEnabled]", actual.Sql);
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