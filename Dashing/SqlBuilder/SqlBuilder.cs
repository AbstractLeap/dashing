using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.SqlBuilder
{
    public enum JoinType
    {
        InnerJoin,
        LeftJoin,
        RightJoin,
        FullOuterJoin
    }

    class SqlBuilder
    {
        private readonly ISession session;

        public SqlBuilder(ISession session)
        {
            this.session = session;
        }

        public ISqlFromDefinition<T> From<T>()
        {
            return new SqlFromDefinition<T>();
        }
    }

    //class SqlQuerySelection<T, TResult> : IEnumerable<TResult>
    //{
    //    private readonly Expression<Func<T, TResult>> selectExpression;

    //    public SqlQuerySelection(Expression<Func<T, TResult>> selectExpression)
    //    {
    //        this.selectExpression = selectExpression;
    //    }

    //    public IEnumerator<TResult> GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //class SqlQuery<T>
    //{
    //    public SqlQuery<T, TSecond> Join<TSecond>()
    //    {
    //        return new SqlQuery<T, TSecond>();
    //    }

    //    public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> selectExpression)
    //    {
    //        return new SqlQuerySelection<T, TResult>(selectExpression);
    //    }
    //}


    /*
     * Select
     * Into
     * From
     * Where
     * Having
     * Group By
     * Order By
     * Paging
     * 
     * With
     * Union
     * Intersect
     * Except
     */
}
