namespace Dashing.SqlBuilder {
    using System.Linq.Expressions;

    public abstract class BaseSqlFromWithJoinDefinition : BaseSqlFromDefinition {
        public JoinType JoinType { get; protected set; }

        public Expression JoinExpression { get; protected set; }

        public BaseSqlFromDefinition PreviousFromDefinition { get; protected set; }
    }
}