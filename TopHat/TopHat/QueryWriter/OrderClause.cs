using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Sql;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TopHat
{
    public class OrderClause<T>
    {
        public Expression Expression { get; private set; }

        public string Clause { get; private set; }

        public ListSortDirection Direction { get; private set; }

        public OrderClause(Expression expression, ListSortDirection direction)
        {
            this.Expression = expression;
            this.Direction = direction;
        }

        public OrderClause(string clause, ListSortDirection direction)
        {
            this.Clause = clause;
            this.Direction = direction;
        }

        public bool IsExpression()
        {
            return this.Expression != null;
        }
    }
}