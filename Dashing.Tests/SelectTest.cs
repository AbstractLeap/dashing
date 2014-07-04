namespace Dashing.Tests {
  using global::Dashing.Tests.TestDomain;

  using Xunit;

  public class SelectTest : BaseQueryWriterTest {
    [Fact]
    public void TestProjection() {
      var Dashing = this.GetDashing();
      var project = Dashing.Query<Blog>().Select(b => new { b.BlogId, b.Title });
      Assert.NotNull((project as SelectQuery<Blog>).Projection);
    }

    [Fact]
    public void FetchAllProperties() {
      var fetchAll = this.GetDashing().Query<Blog>().IncludeAll();
      Assert.True((fetchAll as SelectQuery<Blog>).FetchAllProperties);
    }

    [Fact]
    public void IncludeWorks() {
      var include = this.GetDashing().Query<Post>().Include(p => p.Content);
      Assert.NotEmpty((include as SelectQuery<Post>).Includes);
    }

    [Fact]
    public void ExcludeWorks() {
      var exclude = this.GetDashing().Query<Post>().Exclude(p => p.Content);
      Assert.NotEmpty((exclude as SelectQuery<Post>).Excludes);
    }
  }
}