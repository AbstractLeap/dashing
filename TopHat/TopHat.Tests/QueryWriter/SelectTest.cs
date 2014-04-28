using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.QueryWriter
{
    public class SelectTest : BaseQueryWriterTest
    {
        [Fact]
        public void TestProjection()
        {
            var topHat = GetTopHat();
            var project = topHat.Query<Blog>().Select(b => new { b.BlogId, b.Title });
            Assert.NotNull(project.Query.Project);
        }

        [Fact]
        public void FetchAllProperties()
        {
            var fetchAll = GetTopHat().Query<Blog>().IncludeAll();
            Assert.True(fetchAll.Query.FetchAllProperties);
        }

        [Fact]
        public void IncludeWorks()
        {
            var include = GetTopHat().Query<Post>().Include(p => p.Content);
            Assert.NotEmpty(include.Query.Includes);
        }

        [Fact]
        public void ExcludeWorks()
        {
            var exclude = GetTopHat().Query<Post>().Exclude(p => p.Content);
            Assert.NotEmpty(exclude.Query.Excludes);
        }
    }
}