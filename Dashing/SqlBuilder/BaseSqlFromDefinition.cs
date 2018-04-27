namespace Dashing.SqlBuilder {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public abstract class BaseSqlFromDefinition {
        public IList<Expression> WhereExpressions { get; set; } = new List<Expression>();

        public IList<Expression> HavingExpressions { get; set; } = new List<Expression>();

        public IList<Expression> GroupByExpressions { get; set; } = new List<Expression>();

        public IList<Tuple<Expression, ListSortDirection>> OrderByExpressions { get; set; } = new List<Tuple<Expression, ListSortDirection>>();
    }
}