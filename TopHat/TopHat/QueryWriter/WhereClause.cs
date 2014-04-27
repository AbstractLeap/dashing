using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TopHat
{
    public class WhereClause<T>
    {
        public Expression<Func<T, bool>> Expression { get; private set; }

        public string Clause { get; private set; }

        public dynamic Parameters { get; private set; }

        public WhereClause(Expression<Func<T, bool>> expression)
        {
            this.Expression = expression;
        }

        public WhereClause(string clause)
        {
            this.Clause = clause;
        }

        public WhereClause(string clause, dynamic parameters)
        {
            this.Clause = clause;
            this.Parameters = parameters;
        }

        public bool IsExpression()
        {
            return this.Expression != null;
        }
    }
}