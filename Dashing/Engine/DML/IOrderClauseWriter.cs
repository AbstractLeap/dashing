namespace Dashing.Engine.DML {
    internal interface IOrderClauseWriter {
        string GetOrderClause<T>(OrderClause<T> clause, FetchNode rootNode);
    }
}