namespace Dashing.SqlBuilder {
    using System.Linq.Expressions;

    public abstract class BaseSqlFromWithJoinDefinition : BaseSqlFromDefinition {
        public JoinType JoinType { get; set; }

        public Expression JoinExpression { get; set; }
    }
}