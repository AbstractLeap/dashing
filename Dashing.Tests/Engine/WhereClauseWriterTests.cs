namespace Dashing.Tests.Engine {
    using System.Data;
    using System.Diagnostics;

    using Dashing.Configuration;
    using Dashing.Engine;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class WhereClauseWriterTests {

        [Fact]
        public void TwoWhereClausesStack() {
            var whereClauseWriter = new WhereClauseWriter(new SqlServerDialect(), MakeConfig());

            System.Linq.Expressions.Expression<System.Func<Post, bool>> whereClause1 = p=>p.PostId>0;
            System.Linq.Expressions.Expression<System.Func<Post, bool>> whereClause2 = p=>p.PostId<2;

            System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<Post, bool>>> foo = new System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<Post,bool>>>();
            foo.Add(whereClause1);
            foo.Add(whereClause2);

            var result = whereClauseWriter.GenerateSql(foo, null);
            Debug.Write(result.Sql);
            Assert.Equal(" where ([PostId] > @l_1) and ([PostId] < @l_2)", result.Sql);
        }

        private SelectQuery<T> GetSelectQuery<T>() {
            var engine = new Mock<IEngine>().Object;
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(c => c.State).Returns(ConnectionState.Open);
            var transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            transaction.Setup(m => m.Connection).Returns(connection.Object);
            return new SelectQuery<T>(engine, transaction.Object);
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