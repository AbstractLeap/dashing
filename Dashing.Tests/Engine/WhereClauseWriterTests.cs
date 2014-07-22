namespace Dashing.Tests.Engine {
    using System.Data;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;
    using Dashing.Extensions;

    using Moq;

    using Xunit;

    public class WhereClauseWriterTests {

        [Fact]
        public void TwoWhereClausesStack() {
            var whereClauseWriter = new WhereClauseWriter(new SqlServerDialect(), MakeConfig());

            Expression<System.Func<Post, bool>> whereClause1 = p => p.PostId > 0;
            Expression<System.Func<Post, bool>> whereClause2 = p => p.PostId < 2;

            var foo = new List<Expression<System.Func<Post, bool>>> {
                                                                        whereClause1,
                                                                        whereClause2
                                                                    };

            var result = whereClauseWriter.GenerateSql(foo, null);
            Debug.Write(result.Sql);
            Assert.Equal(" where ([PostId] > @l_1) and ([PostId] < @l_2)", result.Sql);
        }

        [Fact]
        public void TwoWhereClausesParametersOkay() {
            var whereClauseWriter = new WhereClauseWriter(new SqlServerDialect(), MakeConfig());

            Expression<System.Func<Post, bool>> whereClause1 = p => p.PostId > 0;
            Expression<System.Func<Post, bool>> whereClause2 = p => p.PostId < 2;

            var foo = new List<Expression<System.Func<Post, bool>>>();
            foo.Add(whereClause1);
            foo.Add(whereClause2);

            var result = whereClauseWriter.GenerateSql(foo, null);
            Debug.Write(result.Sql);
            Assert.Equal(0, result.Parameters.GetValue("l_1"));
            Assert.Equal(2, result.Parameters.GetValue("l_2"));
        }

        [Fact]
        public void UsesPrimaryKeyWhereEntityEqualsEntity() {
            // assemble
            var whereClauseWriter = new WhereClauseWriter(new SqlServerDialect(), MakeConfig());
            var user = new User() { UserId = 1 };
            Expression<System.Func<User, bool>> whereClause = u => u == user;

            // act
            var actual = whereClauseWriter.GenerateSql(new[] { whereClause }, null);

            // assert
            Assert.Equal(" where ([UserId] = @l_1)", actual.Sql);
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
                this.Setup<Post>().Property(p => p.DoNotMap).Ignore();
            }
        }

    }
}