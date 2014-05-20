namespace TopHat.Tests.Tracking {
  using global::TopHat.Tests.TestDomain;
  using global::TopHat.Tracking;

  using LinFu.Proxy.Interfaces;

  using Xunit;

  public class ProxyTests {
    [Fact]
    public void RegisterFactoryAddsBehaviourToProxy() {
      var behaviour = new ChangeTrackingBehaviourFactory();
      var proxyManager = new ProxyManager();
      proxyManager.Register(behaviour);

      // ReSharper disable once SuspiciousTypeConversion.Global - although this is suspicious its ok
      var linfuproxy = (IProxy)proxyManager.ProxyFor(new Post());
      var proxy = (Proxy<Post>)linfuproxy.Interceptor;
      Assert.True(proxy.Behaviours.ContainsKey(behaviour.Name));
    }
  }
}