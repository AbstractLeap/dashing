namespace TopHat.Tests.Configuration {
  using Moq;

  using global::TopHat.Configuration;

  using Xunit;

  public class DefaultConfigurationTests {
    private const string ConnectionString = "Host=dummy.local";

    [Fact]
    public void Constructs() {
      var engine = new Mock<IEngine>();
      Assert.NotNull(new DefaultConfiguration(engine.Object, ConnectionString));
    }
  }
}