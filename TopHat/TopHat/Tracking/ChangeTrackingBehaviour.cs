namespace TopHat.Tracking {
  using System;
  using System.Collections.Generic;
  using System.Linq;

  using global::TopHat.Extensions;

  using LinFu.AOP.Interfaces;

  public class ChangeTrackingBehaviour : BaseBehaviour {
    public IList<string> DirtyProperties { get; private set; }

    private IDictionary<string, object> OldValues { get; set; }

    public IDictionary<string, IList<object>> AddedEntities { get; private set; }

    public IDictionary<string, IList<object>> DeletedEntities { get; private set; }

    public ChangeTrackingBehaviour() {
      this.DirtyProperties = new List<string>();
      this.OldValues = new Dictionary<string, object>();
      this.AddedEntities = new Dictionary<string, IList<object>>();
      this.DeletedEntities = new Dictionary<string, IList<object>>();
    }

    public override void OnCreate<T>(Proxy<T> proxy) {
      // override any collections with observable collections so that we can track changes
      var collectionProperties = typeof(T).GetProperties().Where(p => p.PropertyType.IsCollection()).ToList();
      foreach (var prop in collectionProperties) {
        var underlyingType = prop.PropertyType.GetGenericArguments()[0];
        var observableCollectionType = typeof(ChangeTrackedCollection<>).MakeGenericType(underlyingType);
        var currentCollection = prop.GetValue(proxy.Entity, null);
        var newCollection = Activator.CreateInstance(observableCollectionType, currentCollection, this, prop.Name);
        prop.SetValue(proxy.Entity, newCollection, null);
      }
    }

    public override void OnSet<T>(Proxy<T> proxy, IInvocationInfo info) {
      if (info.Arguments[0] != null && info.Arguments[0].GetType().IsCollection()) {
        return;
      }

      var propertyName = info.TargetMethod.Name.Substring(4);
      if (this.DirtyProperties.Contains(propertyName)) {
        return;
      }

      var oldValue = proxy.Entity.GetType().GetProperty(propertyName).GetValue(proxy.Entity, null);

      if ((info.Arguments[0] != null || oldValue == null) && (info.Arguments[0] == null || info.Arguments[0].Equals(oldValue))) {
        return;
      }

      this.DirtyProperties.Add(propertyName);
      this.OldValues.Add(propertyName, oldValue);
    }
  }
}