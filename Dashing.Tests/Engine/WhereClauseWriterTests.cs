namespace Dashing.Tests.Engine {
    using System.Data;
    using System.Diagnostics;

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

            System.Linq.Expressions.Expression<System.Func<Post, bool>> whereClause1 = p => p.PostId > 0;
            System.Linq.Expressions.Expression<System.Func<Post, bool>> whereClause2 = p => p.PostId < 2;

            System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<Post, bool>>> foo = new System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<Post, bool>>>();
            foo.Add(whereClause1);
            foo.Add(whereClause2);

            var result = whereClauseWriter.GenerateSql(foo, null);
            Debug.Write(result.Sql);
            Assert.Equal(" where ([PostId] > @l_1) and ([PostId] < @l_2)", result.Sql);
        }

        [Fact]
        public void TwoWhereClausesParametersOkay() {
            var whereClauseWriter = new WhereClauseWriter(new SqlServerDialect(), MakeConfig());

            System.Linq.Expressions.Expression<System.Func<Post, bool>> whereClause1 = p => p.PostId > 0;
            System.Linq.Expressions.Expression<System.Func<Post, bool>> whereClause2 = p => p.PostId < 2;

            System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<Post, bool>>> foo = new System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<Post, bool>>>();
            foo.Add(whereClause1);
            foo.Add(whereClause2);

            var result = whereClauseWriter.GenerateSql(foo, null);
            Debug.Write(result.Sql);
            Assert.Equal(0, result.Parameters.GetValue("l_1"));
            Assert.Equal(2, result.Parameters.GetValue("l_2"));
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