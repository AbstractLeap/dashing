using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class TrackedTest : BaseQueryWriterTest
    {
        [Fact]
        public void CheckMarkedAsTracked()
        {
            var tracked = GetTopHat().QueryTracked<Post>();
            Assert.True(tracked.Query.Tracked);
        }
    }
}