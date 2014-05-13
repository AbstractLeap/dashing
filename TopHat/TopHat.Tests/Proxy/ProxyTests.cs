using LinFu.Proxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Proxy;
using TopHat.Proxy.ChangeTracking;
using TopHat.Tests.TestDomain;
using Xunit;

namespace TopHat.Tests.Proxy
{
    public class ProxyTests
    {
        [Fact]
        public void RegisterFactoryAddsBehaviourToProxy()
        {
            var behaviour = new ChangeTrackingBehaviourFactory();
            var proxyManager = new ProxyManager();
            proxyManager.Register(behaviour);

            var entity = proxyManager.Proxy(new Post());

            var linfuproxy = entity as IProxy;
            var proxy = linfuproxy.Interceptor as Proxy<Post>;
            Assert.True(proxy.Behaviours.ContainsKey(behaviour.Name));
        }
    }
}