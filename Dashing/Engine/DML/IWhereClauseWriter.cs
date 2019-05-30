namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IWhereClauseWriter {
        SelectWriterResult GenerateSql<T>(IEnumerable<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode, AutoNamingDynamicParameters parameters);
    }
}