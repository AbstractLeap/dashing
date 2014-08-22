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

        private class WherePropertyIsDefinedOnInterfaceDemonstrator<T>
            where T : IEnableable {
            private readonly WhereClauseWriter writer;

            private readonly IList<Expression<Func<T, bool>>> whereClauses;

            public WherePropertyIsDefinedOnInterfaceDemonstrator(WhereClauseWriter writer) {
                this.writer = writer;
                this.whereClauses = new List<Expression<Func<T, bool>>> {
                    z => z.IsEnabled
                };
            }

            public SelectWriterResult Execute() {
                return this.writer.GenerateSql(this.whereClauses, null);
            }
        }

        [Fact]
        public void WherePropertyIsDefinedOnInterface() {
            // assemble
            var target = MakeTarget();
            Expression<Func<User, bool>> whereClause = u => u.IsEnabled;

            // act
            var actual = target.GenerateSql(new[] { whereClause }, null);

            // assert
            Assert.Equal(" where [IsEnabled]", actual.Sql);
        }

        [Fact]
        public void WherePredicateIsDefinedOnInterface() {
            // assemble
            var target = MakeTarget();

            // act
            var demonstrator = new WherePropertyIsDefinedOnInterfaceDemonstrator<User>(target);
            var actual = demonstrator.Execute();

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