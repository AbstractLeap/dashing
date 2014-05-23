namespace TopHat.Tracking {
  using System.Collections.Generic;

  public interface IProxyManager {
    /// <summary>
    ///   Generates Proxy for the provided entity
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="entity">The entity to proxy</param>
    /// <returns>The proxy</returns>
    T ProxyFor<T>(T entity);

    /// <summary>
    ///   Returns the list of currently registered behaviours
    /// </summary>
    IEnumerable<IBehaviourFactory> BehaviourFactories { get; }

    /// <summary>
    ///   Registers a new behaviour with the proxy manager
    /// </summary>
    /// <param name="behaviourFactory"></param>
    void Register(IBehaviourFactory behaviourFactory);

    /// <summary>
    ///   Suspend invocation of a particular behaviour on a proxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="behaviourName"></param>
    /// <param name="entity"></param>
    void Suspend<T>(string behaviourName, T entity);

    /// <summary>
    ///   Suspend invocation of all behaviours on a proxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    void Suspend<T>(T entity);

    /// <summary>
    ///   Resume invocation of a particular behaviour on a proxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="behaviourName"></param>
    /// <param name="entity"></param>
    void Resume<T>(string behaviourName, T entity);

    /// <summary>
    ///   Resume invocation of all behaviours on a proxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    void Resume<T>(T entity);
  }
}