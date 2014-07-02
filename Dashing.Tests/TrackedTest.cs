namespace Dashing.Tests {
  using global::Dashing.Tests.TestDomain;

  using Xunit;

  public class TrackedTest : BaseQueryWriterTest {
    [Fact]
    public void CheckMarkedAsTracked() {
      var tracked = this.GetDashing().Query<Post>().AsTracked();
      Assert.True(tracked.IsTracked);
    }
  }
}