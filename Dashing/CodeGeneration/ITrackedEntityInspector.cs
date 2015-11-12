namespace Dashing.CodeGeneration {
    using System;
    using System.Linq.Expressions;

    public interface ITrackedEntityInspector<T> : ITrackedEntity {
        bool IsPropertyDirty<TResult>(Expression<Func<T, TResult>> propertyExpression);

        TResult GetOldValue<TResult>(Expression<Func<T, TResult>> propertyExpression);

        TResult GetNewValue<TResult>(Expression<Func<T, TResult>> propertyExpression);

        object GetNewValue(string propertyName);

        bool IsDirty();
    }
}