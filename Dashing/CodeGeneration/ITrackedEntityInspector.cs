namespace Dashing.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface ITrackedEntityInspector<T> {
        /// <summary>
        ///     Indicates whether this entity is currently being tracked
        /// </summary>
        bool IsTracking { get; }

        /// <summary>
        ///     Returns the set of dirty properties
        /// </summary>
        /// <remarks>Does not include dirty collection properties</remarks>
        IEnumerable<string> DirtyProperties { get; }

        /// <summary>
        ///     Returns the old values for dirty properties
        /// </summary>
        /// <remarks>Does not include dirty collection properties</remarks>
        IDictionary<string, object> OldValues { get; }

        /// <summary>
        ///     Suspends tracking on an entity
        /// </summary>
        void EnableTracking();

        /// <summary>
        ///     Resumes tracking on an entity
        /// </summary>
        void DisabledTracking();

        /// <summary>
        ///     Indicates if the entity has any dirty properties or dirty collections
        /// </summary>
        /// <returns></returns>
        bool IsDirty();

        /// <summary>
        ///     Indicates whether a particular property is dirty
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        bool IsPropertyDirty<TResult>(Expression<Func<T, TResult>> propertyExpression);

        /// <summary>
        /// Indicates whether a particular property is dirty
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        bool IsPropertyDirty(string propertyName);

        /// <summary>
        ///     Fetches the old value for a dirty property
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        TResult OldValueFor<TResult>(Expression<Func<T, TResult>> propertyExpression);

        /// <summary>
        /// Fetches the new value for a property by string
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        object NewValueFor(string propertyName);
    }
}