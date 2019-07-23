namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IBulkUpdateWriter {
        SqlWriterResult GenerateBulkSql<T>(Action<T> updateAction, IEnumerable<Expression<Func<T, bool>>> predicates)
            where T : class, new();
    }
}