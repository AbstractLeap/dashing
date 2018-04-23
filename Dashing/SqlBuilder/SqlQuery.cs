using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dashing.SqlBuilder
{
    public class SqlQuerySelection<T, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> : IEnumerable<TResult>
    {
        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> selectExpression;
        public SqlQuerySelection(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> selectExpression)
        {
            this.selectExpression = selectExpression;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SqlFromDefinition<T>
    {

        private readonly IList<Expression<Func<T, bool>>> whereExpressions = new List<Expression<Func<T, bool>>>();

        private readonly IList<Expression<Func<T, bool>>> havingExpressions = new List<Expression<Func<T, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();

        public SqlFromDefinition<T, T2> InnerJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2> InnerJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2> LeftJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2> LeftJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2> RightJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2> RightJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2> FullOuterJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2> FullOuterJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T> Where(Expression<Func<T, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T> Having(Expression<Func<T, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T> GroupBy<TResult>(Expression<Func<T, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T> OrderBy<TResult>(Expression<Func<T, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2>
    {

        private readonly IList<Expression<Func<T, T2, bool>>> whereExpressions = new List<Expression<Func<T, T2, bool>>>();

        private readonly IList<Expression<Func<T, T2, bool>>> havingExpressions = new List<Expression<Func<T, T2, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3> InnerJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3> InnerJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3> LeftJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3> LeftJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3> RightJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3> RightJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3> FullOuterJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3> FullOuterJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2> Where(Expression<Func<T, T2, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2> Having(Expression<Func<T, T2, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2> GroupBy<TResult>(Expression<Func<T, T2, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2> OrderBy<TResult>(Expression<Func<T, T2, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3>
    {

        private readonly IList<Expression<Func<T, T2, T3, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4> InnerJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4> InnerJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4> LeftJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4> LeftJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4> RightJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4> RightJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4> FullOuterJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4> FullOuterJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3> Having(Expression<Func<T, T2, T3, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3> GroupBy<TResult>(Expression<Func<T, T2, T3, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3> OrderBy<TResult>(Expression<Func<T, T2, T3, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> InnerJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> LeftJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> RightJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> RightJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> FullOuterJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> FullOuterJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4> Having(Expression<Func<T, T2, T3, T4, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> InnerJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> InnerJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> LeftJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> LeftJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> RightJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> RightJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> FullOuterJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> FullOuterJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> Where(Expression<Func<T, T2, T3, T4, T5, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> Having(Expression<Func<T, T2, T3, T4, T5, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> FullOuterJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> FullOuterJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> Where(Expression<Func<T, T2, T3, T4, T5, T6, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> Having(Expression<Func<T, T2, T3, T4, T5, T6, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> FullOuterJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> FullOuterJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> FullOuterJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> FullOuterJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> FullOuterJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> FullOuterJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FullOuterJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FullOuterJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FullOuterJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FullOuterJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FullOuterJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FullOuterJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FullOuterJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FullOuterJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FullOuterJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FullOuterJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.InnerJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.InnerJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.LeftJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.LeftJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.RightJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.RightJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FullOuterJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.FullOuterJoin);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FullOuterJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(JoinType.FullOuterJoin, joinExpression);
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(selectExpression);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    {

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> whereExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>>();

        private readonly IList<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> havingExpressions = new List<Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>>();

        private readonly IList<Expression> groupByExpressions = new List<Expression>();

        private readonly IList<Tuple<Expression, ListSortDirection>> orderByExpressions = new List<Tuple<Expression, ListSortDirection>>();


        private readonly JoinType joinType;

        private readonly Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression;

        public SqlFromDefinition(JoinType joinType)
        {
            this.joinType = joinType;
        }

        public SqlFromDefinition(JoinType joinType, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            this.joinType = joinType;
            this.joinExpression = joinExpression;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> whereExpression)
        {
            this.whereExpressions.Add(whereExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> havingExpression)
        {
            this.havingExpressions.Add(havingExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> groupByExpression)
        {
            this.groupByExpressions.Add(groupByExpression);
            return this;
        }
        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.orderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }


        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> selectExpression)
        {
            return new SqlQuerySelection<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(selectExpression);
        }
    }




}
