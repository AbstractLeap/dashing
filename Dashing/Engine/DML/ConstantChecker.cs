using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.Engine.DML {
    internal class ConstantChecker : BaseExpressionVisitor {
        public bool HasParams { get; set; }

        public void Reset() {
            this.HasParams = false;
        }

        protected override Expression Visit(Expression exp) {
            return base.Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p) {
            this.HasParams = true;
            return p;
        }
    }
}
