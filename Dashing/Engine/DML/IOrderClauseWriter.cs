namespace Dashing.Engine.DML {
    using System;

    using Dashing.Configuration;

    internal interface IOrderClauseWriter {
        string GetOrderClause<T>(
            OrderClause<T> clause, 
            QueryTree rootQueryNode,
            IAliasProvider aliasProvider, 
            out bool isRootPrimaryKeyClause);

        string GetOrderClause<T>(
            OrderClause<T> clause,
            QueryTree rootQueryNode,
            IAliasProvider aliasProvider,
            Func<IColumn, BaseQueryNode, string> aliasRewriter,
            Func<IColumn, BaseQueryNode, string> nameRewriter,
            out bool isRootPrimaryKeyClause);
    }
}