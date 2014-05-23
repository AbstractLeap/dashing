namespace TopHat.Tracking {
  using LinFu.AOP.Interfaces;

  public abstract class BaseBehaviour : IBehaviour {
    public virtual void OnGet<T>(Proxy<T> proxy, IInvocationInfo info) { }

    public virtual void OnSet<T>(Proxy<T> proxy, IInvocationInfo info) { }

    public virtual void OnInvoke<T>(Proxy<T> proxy, IInvocationInfo info) { }

    public virtual void OnCreate<T>(Proxy<T> proxy) { }
  }
}