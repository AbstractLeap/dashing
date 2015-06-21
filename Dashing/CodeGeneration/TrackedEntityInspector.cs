namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Dashing.Extensions;

    public class TrackedEntityInspector<T> : ITrackedEntityInspector<T> {
        private T entity;

        private readonly ITrackedEntity trackedEntity;

        private static IDictionary<string, Func<T, object>> propertyAccessorCache = new Dictionary<string, Func<T, object>>();

        static TrackedEntityInspector() {
            // fill in the property accessor cache
            foreach (var property in typeof(T).GetProperties()) {
                propertyAccessorCache.Add(property.Name, Expression.Lambda<Func<T, object>>(Expression.Property(Expression.Parameter(typeof(T)), property.Name), Expression.Parameter(typeof(T))).Compile());
            }
        }

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
                return this.trackedEntity.IsTrackingEnabled();
            }
        }

        public IEnumerable<string> DirtyProperties {
            get {
                return this.trackedEntity.GetDirtyProperties();
            }
        }

        public IDictionary<string, object> OldValues {
            get {
                return this.trackedEntity.GetDirtyProperties().ToDictionary(p => p, p => this.trackedEntity.GetOldValue(p));
            }
        }

        public void EnableTracking() {
            this.trackedEntity.EnableTracking();
        }

        public void DisabledTracking() {
            this.trackedEntity.DisableTracking();
        }

        public bool IsDirty() {
            return this.DirtyProperties.Any();
        }

        public bool IsPropertyDirty<TResult>(Expression<Func<T, TResult>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            return this.trackedEntity.GetDirtyProperties().Any(p => p == memberExpr.Member.Name);
        }

        public bool IsPropertyDirty(string propertyName) {
            return this.trackedEntity.GetDirtyProperties().Any(p => p == propertyName);
        }

        public TResult OldValueFor<TResult>(Expression<Func<T, TResult>> propertyExpression) {
            var memberExpr = propertyExpression.Body as MemberExpression;
            if (memberExpr == null) {
                throw new ArgumentException("The propertyExpression must be a MemberExpression", "propertyExpression");
            }

            if (!this.trackedEntity.GetDirtyProperties().Any(p => p == memberExpr.Member.Name)) {
                throw new ArgumentException("This property is not dirty. Please check IsDirty before asking for old value");
            }

            return (TResult)this.OldValues[memberExpr.Member.Name];
        }


        public object NewValueFor(string propertyName) {
            return propertyAccessorCache[propertyName](this.entity);
        }
    }
}