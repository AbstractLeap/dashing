namespace TopHat.Tests {
  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class FetchTest : BaseQueryWriterTest {
    [Fact]
    public void SingleLevelFetch() {
      var topHat = this.GetTopHat();
      var fetch = topHat.Query<Post>().Fetch(p => p.Blog);
      Assert.NotEmpty(fetch.Query.Fetches);
    }

    [Fact]
    public void MultipleLevelFetch() {
      var topHat = this.GetTopHat();
      var fetch = topHat.Query<Comment>().Fetch(c => c.Post).ThenFetch(p => p.Blog);
      Assert.Equal(2, fetch.Query.Fetches.First().Count);
    }
  }
}