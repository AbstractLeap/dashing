namespace TopHat.Tests {
  using global::TopHat.Tests.TestDomain;

  using Xunit;

  public class TrackedTest : BaseQueryWriterTest {
    [Fact]
    public void CheckMarkedAsTracked() {
      var tracked = this.GetTopHat().Query<Post>().AsTracked();
      Assert.True(tracked.IsTracked);
    }
  }
}