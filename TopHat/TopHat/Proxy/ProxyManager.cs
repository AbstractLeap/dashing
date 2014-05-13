using LinFu.AOP.Interfaces;
using LinFu.Proxy;
using LinFu.Proxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Proxy
{
    public class ProxyManager : IProxyManager
    {
        private static ProxyFactory proxyFactory = new ProxyFactory();

        private IList<IBehaviourFactory> behaviourFactories;

        public IList<IBehaviourFactory> BehaviourFactories
        {
            get { return this.behaviourFactories; }
        }

        public ProxyManager()
        {
            this.behaviourFactories = new List<IBehaviourFactory>();
        }

        public T Proxy<T>(T entity)
        {
            var proxy = new Proxy<T>(entity, this);

            return proxyFactory.CreateProxy<T>(proxy);
        }

        public void Register(IBehaviourFactory behaviour)
        {
            this.behaviourFactories.Add(behaviour);
        }

        public void Suspend<T>(string behaviourName, T entity)
        {
            this.GetInterceptor(entity).Suspend(behaviourName);
        }

        public void Suspend<T>(T entity)
        {
            this.GetInterceptor(entity).Suspend();
        }

        public void Resume<T>(string behaviourName, T entity)
        {
            this.GetInterceptor(entity).Resume(behaviourName);
        }

        public void Resume<T>(T entity)
        {
            this.GetInterceptor(entity).Resume();
        }

        private Proxy<T> GetInterceptor<T>(T entity)
        {
            var proxy = entity as IProxy;
            if (proxy == null)
            {
                throw new ArgumentException("The entity passed to GetInterceptor is not a proxy");
            }

            return proxy.Interceptor as Proxy<T>;
        }
    }
}