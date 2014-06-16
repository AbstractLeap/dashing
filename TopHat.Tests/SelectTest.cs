namespace TopHat.Tests {
  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class SelectTest : BaseQueryWriterTest {
    [Fact]
    public void TestProjection() {
      var topHat = this.GetTopHat();
      var project = topHat.Query<Blog>().Select(b => new { b.BlogId, b.Title });
      Assert.NotNull((project as SelectQuery<Blog>).Projection);
    }

    [Fact]
    public void FetchAllProperties() {
      var fetchAll = this.GetTopHat().Query<Blog>().IncludeAll();
      Assert.True((fetchAll as SelectQuery<Blog>).FetchAllProperties);
    }

    [Fact]
    public void IncludeWorks() {
      var include = this.GetTopHat().Query<Post>().Include(p => p.Content);
      Assert.NotEmpty((include as SelectQuery<Post>).Includes);
    }

    [Fact]
    public void ExcludeWorks() {
      var exclude = this.GetTopHat().Query<Post>().Exclude(p => p.Content);
      Assert.NotEmpty((exclude as SelectQuery<Post>).Excludes);
    }
  }
}