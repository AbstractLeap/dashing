namespace Dashing.Engine.DML {
    public interface IAliasProvider {
        string GetAlias(BaseQueryNode queryNode);
    }
}