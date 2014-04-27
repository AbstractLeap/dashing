using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class QueryTest : BaseQueryWriterTest
    {
        [Fact]
        public void ForUpdateSet()
        {
            GetTopHat().Query<Post>().Where(p => p.PostId == 1).ForUpdate();
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.ForUpdate)));
        }

        [Fact]
        public void SkipSet()
        {
            GetTopHat().Query<Post>().Skip(10);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.Skip == 10)));
        }

        [Fact]
        public void TakeSet()
        {
            GetTopHat().Query<Post>().Take(10);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Select && q.Take == 10)));
        }
    }
}