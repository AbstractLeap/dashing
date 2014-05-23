using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace TopHat.CodeGeneration
{
    public sealed class TrackingCollection<TEntity, TProperty> : ObservableCollection<TProperty> where TEntity : ITrackedEntity
    {
        private TEntity entity;

        private string propertyName;

        public TrackingCollection(TEntity entity, string propertyName)
            : this(entity, propertyName, new List<TProperty>())
        { }

        public TrackingCollection(TEntity entity, string propertyName, IEnumerable<TProperty> collection)
            : base(collection)
        {
            this.entity = entity;
            this.propertyName = propertyName;

            this.entity.AddedEntities.Add(this.propertyName, new List<object>());
            this.entity.DeletedEntities.Add(this.propertyName, new List<object>());

            this.CollectionChanged += TrackingCollection_CollectionChanged;
        }

        private void TrackingCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.entity.IsTracking)
            {
                if (e.OldItems != null)
                {
                    foreach (var entity in e.OldItems)
                    {
                        this.entity.DeletedEntities[this.propertyName].Add(entity);
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var entity in e.NewItems)
                    {
                        this.entity.AddedEntities[this.propertyName].Add(entity);
                    }
                }
            }
        }
    }
}