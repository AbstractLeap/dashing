using System.Linq;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class FetchTest : BaseQueryWriterTest
    {
        [Fact]
        public void SingleLevelFetch()
        {
            var topHat = GetTopHat();
            var fetch = topHat.Query<Post>().Fetch(p => p.Blog);
            Assert.NotEmpty(fetch.Query.Fetches);
        }

        [Fact]
        public void MultipleLevelFetch()
        {
            var topHat = GetTopHat();
            var fetch = topHat.Query<Comment>().Fetch(c => c.Post).ThenFetch(p => p.Blog);
            Assert.Equal(2, fetch.Count());
        }
    }
}