using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TopHat
{
    public class Query<T>
    {
        public QueryType QueryType { get; set; }

        public Expression<Func<T, dynamic>> Project { get; private set; }

        public IList<Expression> Includes { get; private set; }

        public IList<Expression> Excludes { get; private set; }

        public IList<IList<Expression>> Fetches { get; private set; }

        public Queue<OrderClause<T>> OrderClauses { get; private set; }

        public IList<WhereClause<T>> WhereClauses { get; private set; }

        public int Skip { get; private set; }

        public int Take { get; private set; }

        public T Entity { get; set; }

        public bool ForUpdate { get; set; }

        public bool FetchAllProperties { get; set; }

        public bool Tracked { get; set; }
    }

    public enum QueryType
    {
        Insert,
        Update,
        Delete,
        Select,
        Naked
    }
}