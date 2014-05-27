namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IWhereClauseWriter {
        SqlWriterResult GenerateSql<T>(IList<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode);
    }
}