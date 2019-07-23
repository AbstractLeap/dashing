namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IBulkDeleteWriter {
        SqlWriterResult GenerateBulkSql<T>(IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}