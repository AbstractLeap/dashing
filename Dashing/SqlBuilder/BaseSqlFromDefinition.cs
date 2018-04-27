namespace Dashing.SqlBuilder {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public abstract class BaseSqlFromDefinition {
        public IList<Expression> WhereExpressions { get; } = new List<Expression>();

        public IList<Expression> HavingExpressions { get; } = new List<Expression>();

        public IList<Expression> GroupByExpressions { get; } = new List<Expression>();

        public IList<Tuple<Expression, ListSortDirection>> OrderByExpressions { get; } = new List<Tuple<Expression, ListSortDirection>>();
    }
}