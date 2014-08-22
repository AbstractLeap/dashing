namespace Dashing.Engine.DML {
    using System.Linq.Expressions;

    internal class ConstantChecker : BaseExpressionVisitor {
        public bool HasParams { get; set; }

        public void Reset() {
            this.HasParams = false;
        }

        protected override Expression VisitParameter(ParameterExpression p) {
            this.HasParams = true;
            return p;
        }
    }
}