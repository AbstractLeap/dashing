namespace Dashing.Engine.DML {
    using System;

    using Dashing.Configuration;

    internal interface IOrderClauseWriter {
        string GetOrderClause<T>(OrderClause<T> clause, FetchNode rootNode, out bool isRootPrimaryKeyClause);

        string GetOrderClause<T>(OrderClause<T> clause, FetchNode rootNode, Func<IColumn, FetchNode, string> aliasRewriter, Func<IColumn, FetchNode, string> nameRewriter, out bool isRootPrimaryKeyClause);
    }
}