namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Extensions;

    public class TrackedEntityInspector<T> : ITrackedEntityInspector<T> {
        private T entity;

        private readonly ITrackedEntity trackedEntity;

        public TrackedEntityInspector(T entity) {
            if (!TryGetAsTracked(entity, out this.trackedEntity)) {
                throw new ArgumentException("This entity is not an ITrackedEntity");
            }

            this.entity = entity;
        }

        public static bool IsTracked(T entity) {
            ITrackedEntity dontcare;
            return TryGetAsTracked(entity, out dontcare);
        }

        public static bool TryGetAsTracked(T entity, out ITrackedEntity trackedEntity) {
            trackedEntity = entity as ITrackedEntity;
            return trackedEntity != null;
        }

        public bool IsTracking {
            get {
                return this.trackedEntity.IsTracking;
            }
        }

        public ISet<string> DirtyProperties {
            get {
                return this.trackedEntity.DirtyProperties;
            }
        }

        public IDictionary<string, object> OldValues {
            get {
                return this.trackedEntity.OldValues;
            }
        }

        public IDictionary<string, object> NewValues {
            get {
                return this.trackedEntity.NewValues;
            }
        }

        public IDictionary<string, IList<object>> AddedEntities {
            get {
                return this.trackedEntity.AddedEntities;
            }
        }

        public IDictionary<string, IList<object>> DeletedEntities {
            get {
                return this.trackedEntity.DeletedEntities;
            }
        }

        public void SuspendTracking() {
            this.trackedEntity.IsTracking = false;
        }

        public void ResumeTracking() {
            this.trackedEntity.IsTracking = true;
        }

        public bool IsDirty() {
            return this.DirtyProperties.Any() || this.AddedEntities.SelectMany(e => e.Value).Any() || this.DeletedEntities.SelectMany(e => e.Value).Any();
        }

        public bool HasOnlyDirtyCollections() {
            return this.IsDirty() && this.DirtyProperties.IsEmpty();
        }

        public bool IsPropertyDirty<TResult>(Expression<Func<T, TResult>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            return this.IsDirtySimple(memberExpr.Member.Name) || (this.AddedEntities.ContainsKey(memberExpr.Member.Name) && this.AddedEntities[memberExpr.Member.Name].Any())
                   || (this.DeletedEntities.ContainsKey(memberExpr.Member.Name) && this.DeletedEntities[memberExpr.Member.Name].Any());
        }

        private bool IsDirtySimple(string name) {
            return this.DirtyProperties.Contains(name);
        }

        public TResult OldValueFor<TResult>(Expression<Func<T, TResult>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            if (!this.IsDirtySimple(memberExpr.Member.Name)) {
                throw new ArgumentException("This property is not dirty. Please check IsDirty before asking for old value");
            }

            return (TResult)this.OldValues[memberExpr.Member.Name];
        }

        public TResult NewValueFor<TResult>(Expression<Func<T, TResult>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            if (!this.IsDirtySimple(memberExpr.Member.Name)) {
                throw new ArgumentException("This property is not dirty. Please check IsDirty before asking for new value");
            }

            return (TResult)this.NewValues[memberExpr.Member.Name];
        }

        public IEnumerable<TResult> AddedEntitiesFor<TResult>(Expression<Func<T, IEnumerable<TResult>>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            return (IEnumerable<TResult>)this.AddedEntities[memberExpr.Member.Name];
        }

        public IEnumerable<TResult> DeletedEntitiesFor<TResult>(Expression<Func<T, IEnumerable<TResult>>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            return (IEnumerable<TResult>)this.AddedEntities[memberExpr.Member.Name];
        }
    }
}