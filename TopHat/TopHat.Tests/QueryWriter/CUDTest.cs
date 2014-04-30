using Moq;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class CUDTest : BaseQueryWriterTest
    {
        [Fact]
        public void InsertGivesGoodQuery()
        {
            var post = new Post { Title = "Hello" };
            GetTopHat().Insert(post);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.Entity.Equals(post) && q.QueryType == QueryType.Insert)));
        }

        [Fact]
        public void UpdateGivesGoodQuery()
        {
            var post = new Post { Title = "Hello", PostId = 1 };
            GetTopHat().Update(post);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.Entity.Equals(post) && q.QueryType == QueryType.Update)));
        }

        [Fact]
        public void DeleteEntityGivesGoodQuery()
        {
            var post = new Post { Title = "Hello", PostId = 1 };
            GetTopHat().Delete(post);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.Entity.Equals(post) && q.QueryType == QueryType.Delete)));
        }

        [Fact]
        public void DeleteByIdGivesGoodQuery()
        {
            GetTopHat().Delete<Post>(1);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.Entity.PostId == 1 && q.QueryType == QueryType.Delete)));
        }

        [Fact]
        public void WhereClauseUpdateExecutes()
        {
            GetTopHat().Delete<Post>().Where(p => p.PostId < 5);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Delete && q.WhereClauses.Count == 1)));
        }

        [Fact]
        public void WhereClauseDeleteExecutes()
        {
            GetTopHat().Update<Post>().Where(p => p.PostId < 5);
            this.sql.Verify(s => s.Execute<Post>(It.Is<Query<Post>>(q => q.QueryType == QueryType.Update && q.WhereClauses.Count == 1)));
        }
    }
}