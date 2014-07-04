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
        ISet<string> DirtyProperties { get; }

        /// <summary>
        ///     Returns the old values for dirty properties
        /// </summary>
        /// <remarks>Does not include dirty collection properties</remarks>
        IDictionary<string, object> OldValues { get; }

        /// <summary>
        ///     Returns the new values for dirty properties
        /// </summary>
        /// <remarks>Does not include dirty collection properties</remarks>
        IDictionary<string, object> NewValues { get; }

        /// <summary>
        ///     Returns a dictionary of collection type properties and the entities added to them
        /// </summary>
        IDictionary<string, IList<object>> AddedEntities { get; }

        /// <summary>
        ///     Returns a dictionary of collection type properties and the entities removed from them
        /// </summary>
        IDictionary<string, IList<object>> DeletedEntities { get; }

        /// <summary>
        ///     Suspends tracking on an entity
        /// </summary>
        void SuspendTracking();

        /// <summary>
        ///     Resumes tracking on an entity
        /// </summary>
        void ResumeTracking();

        /// <summary>
        ///     Indicates if the entity has any dirty properties or dirty collections
        /// </summary>
        /// <returns></returns>
        bool IsDirty();

        /// <summary>
        ///     Indicates if the entity only has dirty collection properties
        /// </summary>
        /// <returns></returns>
        bool HasOnlyDirtyCollections();

        /// <summary>
        ///     Indicates whether a particular property is dirty
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        bool IsPropertyDirty<TResult>(Expression<Func<T, TResult>> propertyExpression);

        /// <summary>
        ///     Fetches the old value for a dirty property
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        TResult OldValueFor<TResult>(Expression<Func<T, TResult>> propertyExpression);

        /// <summary>
        ///     Fetches the new value for a dirty property
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        TResult NewValueFor<TResult>(Expression<Func<T, TResult>> propertyExpression);

        /// <summary>
        ///     Fetches the added entities for a property
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        IEnumerable<TResult> AddedEntitiesFor<TResult>(Expression<Func<T, IEnumerable<TResult>>> propertyExpression);

        /// <summary>
        ///     Fetches the deleted entitites for a property
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        IEnumerable<TResult> DeletedEntitiesFor<TResult>(Expression<Func<T, IEnumerable<TResult>>> propertyExpression);
    }
}