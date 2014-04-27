using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class ProjectTest : BaseQueryWriterTest
    {
        [Fact]
        public void TestProjection()
        {
            var topHat = GetTopHat();
            var project = topHat.Query<Blog>().Project(b => new { b.BlogId, b.Title });
            Assert.NotNull(project.Query.Project);
        }

        [Fact]
        public void FetchAllProperties()
        {
            var fetchAll = GetTopHat().Query<Blog>().FetchAllProperties();
            Assert.True(fetchAll.Query.FetchAllProperties);
        }
    }
}