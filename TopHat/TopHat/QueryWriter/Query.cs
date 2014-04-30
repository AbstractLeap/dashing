using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TopHat
{
    public class Query<T>
    {
        public Query()
        {
            this.Includes = new List<Expression>();
            this.Excludes = new List<Expression>();
            this.Fetches = new List<IList<Expression>>();
            this.OrderClauses = new Queue<OrderClause<T>>();
            this.WhereClauses = new List<WhereClause<T>>();
        }

        public QueryType QueryType { get; set; }

        public Expression<Func<T, dynamic>> Project { get; set; }

        public IList<Expression> Includes { get; set; }

        public IList<Expression> Excludes { get; set; }

        public IList<IList<Expression>> Fetches { get; set; }

        public Queue<OrderClause<T>> OrderClauses { get; set; }

        public IList<WhereClause<T>> WhereClauses { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

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