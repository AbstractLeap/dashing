namespace TopHat.Tracking {
  using LinFu.AOP.Interfaces;

  public interface IBehaviour {
    void OnGet<T>(Proxy<T> proxy, IInvocationInfo info);

    void OnSet<T>(Proxy<T> proxy, IInvocationInfo info);

    void OnInvoke<T>(Proxy<T> proxy, IInvocationInfo info);

    void OnCreate<T>(Proxy<T> proxy);
  }
}