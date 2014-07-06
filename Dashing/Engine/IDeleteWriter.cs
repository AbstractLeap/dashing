namespace Dashing.Engine {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IDeleteWriter : IEntitySqlWriter {
        SqlWriterResult GenerateBulkSql<T>(IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}