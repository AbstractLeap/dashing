namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IUpdateWriter : IEntitySqlWriter {
        SqlWriterResult GenerateBulkSql<T>(T updateClass, IEnumerable<Expression<Func<T, bool>>> predicates);
    }
}