namespace TopHat.Tracking {
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using LinFu.AOP.Interfaces;

  public class Proxy<T> : IInterceptor {
    private readonly IList<string> suspendedBehaviours;

    private bool suspendAllBehaviours;

    public IDictionary<string, IBehaviour> Behaviours { get; private set; }

    public T Entity { get; private set; }

    public Proxy(T entity, IProxyManager proxyManager) {
      this.Entity = entity;
      this.suspendedBehaviours = new List<string>();

      // generate all the behaviour classes
      this.Behaviours = new Dictionary<string, IBehaviour>();
      foreach (var behaviourFactory in proxyManager.BehaviourFactories) {
        var behaviour = behaviourFactory.GetBehaviour();
        behaviour.OnCreate(this);
        this.Behaviours.Add(behaviourFactory.Name, behaviour);
      }
    }

    public object Intercept(IInvocationInfo info) {
      if (!this.suspendAllBehaviours) {
        foreach (var behavour in this.Behaviours.Where(behavour => !this.suspendedBehaviours.Contains(behavour.Key))) {
          if (info.TargetMethod.IsSpecialName) {
            if (info.TargetMethod.Name.StartsWith("set_", StringComparison.OrdinalIgnoreCase)) {
              behavour.Value.OnSet(this, info);
            }
            else if (info.TargetMethod.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase)) {
              behavour.Value.OnGet(this, info);
            }
          }
          else {
            behavour.Value.OnInvoke(this, info);
          }
        }
      }

      return info.TargetMethod.Invoke(this.Entity, info.Arguments);
    }

    public void Suspend() {
      this.suspendAllBehaviours = true;
    }

    public void Suspend(string behaviourName) {
      this.suspendedBehaviours.Add(behaviourName);
    }

    public void Resume() {
      this.suspendAllBehaviours = false;
      this.suspendedBehaviours.Clear();
    }

    public void Resume(string behaviourName) {
      this.suspendedBehaviours.Remove(behaviourName);
    }
  }
}