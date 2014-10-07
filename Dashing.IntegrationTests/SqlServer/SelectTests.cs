using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.SqlServer {
    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class SelectTests : IUseFixture<SqlServerFixture> {
        private SqlServerFixture fixture;

        [Fact]
        public void WhereEnumerableWorks() {
            var ids = new[] { 1, 2, 3 };
            var posts = this.fixture.Session.Query<Post>().Where(p => ids.Contains(p.PostId)).ToList();
            Assert.Equal(3, posts.Count);
        }

        public void SetFixture(SqlServerFixture data) {
            this.fixture = data;
        }
    }
}
