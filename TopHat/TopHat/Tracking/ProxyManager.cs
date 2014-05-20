namespace TopHat.Tracking {
  using System;
  using System.Collections.Generic;

  using LinFu.Proxy;
  using LinFu.Proxy.Interfaces;

  public class ProxyManager : IProxyManager {
    private static readonly ProxyFactory ProxyFactory = new ProxyFactory();

    private readonly IList<IBehaviourFactory> behaviourFactories;

    public IEnumerable<IBehaviourFactory> BehaviourFactories {
      get {
        return this.behaviourFactories;
      }
    }

    public ProxyManager() {
      this.behaviourFactories = new List<IBehaviourFactory>();
    }

    public T ProxyFor<T>(T entity) {
      return ProxyFactory.CreateProxy<T>(new Proxy<T>(entity, this));
    }

    public void Register(IBehaviourFactory behaviour) {
      this.behaviourFactories.Add(behaviour);
    }

    public void Suspend<T>(string behaviourName, T entity) {
      GetInterceptor(entity).Suspend(behaviourName);
    }

    public void Suspend<T>(T entity) {
      GetInterceptor(entity).Suspend();
    }

    public void Resume<T>(string behaviourName, T entity) {
      GetInterceptor(entity).Resume(behaviourName);
    }

    public void Resume<T>(T entity) {
      GetInterceptor(entity).Resume();
    }

    private static Proxy<T> GetInterceptor<T>(T entity) {
      var proxy = entity as IProxy;
      if (proxy == null) {
        throw new ArgumentException("The entity passed to GetInterceptor is not a proxy");
      }

      return (Proxy<T>)proxy.Interceptor;
    }
  }
}