namespace TopHat.Tracking {
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Collections.Specialized;

  internal sealed class ChangeTrackedCollection<T> : ObservableCollection<T> {
    private readonly ChangeTrackingBehaviour behaviour;

    private readonly string propertyName;

    public ChangeTrackedCollection(IEnumerable<T> collection, ChangeTrackingBehaviour behaviour, string propertyName)
      : base(collection ?? new List<T>()) {
      this.behaviour = behaviour;
      this.propertyName = propertyName;

      // add this property in to the behaviour
      this.behaviour.AddedEntities.Add(propertyName, new List<object>());
      this.behaviour.DeletedEntities.Add(propertyName, new List<object>());

      // start listening to change events
      this.CollectionChanged += this.ChangeTrackedCollectionCollectionChanged;
    }

    private void ChangeTrackedCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      // who cares about the action, remove the old items, insert the new items
      if (e.OldItems != null) {
        foreach (var entity in e.OldItems) {
          this.behaviour.DeletedEntities[this.propertyName].Add(entity);
        }
      }

      if (e.NewItems != null) {
        foreach (var entity in e.NewItems) {
          this.behaviour.AddedEntities[this.propertyName].Add(entity);
        }
      }
    }
  }
}