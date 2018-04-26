using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dashing.SqlBuilder
{
    
public interface ISqlFromDefinition<T>
    {

        ISqlFromDefinition<T, T2> InnerJoin<T2>();

        ISqlFromDefinition<T, T2> InnerJoin<T2>(Expression<Func<T, T2, bool>> joinExpression);

        ISqlFromDefinition<T, T2> LeftJoin<T2>();

        ISqlFromDefinition<T, T2> LeftJoin<T2>(Expression<Func<T, T2, bool>> joinExpression);

        ISqlFromDefinition<T, T2> RightJoin<T2>();

        ISqlFromDefinition<T, T2> RightJoin<T2>(Expression<Func<T, T2, bool>> joinExpression);

        ISqlFromDefinition<T, T2> FullOuterJoin<T2>();

        ISqlFromDefinition<T, T2> FullOuterJoin<T2>(Expression<Func<T, T2, bool>> joinExpression);

        ISqlFromDefinition<T> Where(Expression<Func<T, bool>> whereExpression);

        ISqlFromDefinition<T> Having(Expression<Func<T, bool>> havingExpression);

        ISqlFromDefinition<T> GroupBy<TResult>(Expression<Func<T, TResult>> groupByExpression);

        ISqlFromDefinition<T> OrderBy<TResult>(Expression<Func<T, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2>
    {

        ISqlFromDefinition<T, T2, T3> InnerJoin<T3>();

        ISqlFromDefinition<T, T2, T3> InnerJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3> LeftJoin<T3>();

        ISqlFromDefinition<T, T2, T3> LeftJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3> RightJoin<T3>();

        ISqlFromDefinition<T, T2, T3> RightJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3> FullOuterJoin<T3>();

        ISqlFromDefinition<T, T2, T3> FullOuterJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression);

        ISqlFromDefinition<T, T2> Where(Expression<Func<T, T2, bool>> whereExpression);

        ISqlFromDefinition<T, T2> Having(Expression<Func<T, T2, bool>> havingExpression);

        ISqlFromDefinition<T, T2> GroupBy<TResult>(Expression<Func<T, T2, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2> OrderBy<TResult>(Expression<Func<T, T2, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3>
    {

        ISqlFromDefinition<T, T2, T3, T4> InnerJoin<T4>();

        ISqlFromDefinition<T, T2, T3, T4> InnerJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4> LeftJoin<T4>();

        ISqlFromDefinition<T, T2, T3, T4> LeftJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4> RightJoin<T4>();

        ISqlFromDefinition<T, T2, T3, T4> RightJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4> FullOuterJoin<T4>();

        ISqlFromDefinition<T, T2, T3, T4> FullOuterJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3> Having(Expression<Func<T, T2, T3, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3> GroupBy<TResult>(Expression<Func<T, T2, T3, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3> OrderBy<TResult>(Expression<Func<T, T2, T3, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5> InnerJoin<T5>();

        ISqlFromDefinition<T, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> LeftJoin<T5>();

        ISqlFromDefinition<T, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> RightJoin<T5>();

        ISqlFromDefinition<T, T2, T3, T4, T5> RightJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> FullOuterJoin<T5>();

        ISqlFromDefinition<T, T2, T3, T4, T5> FullOuterJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4> Having(Expression<Func<T, T2, T3, T4, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> InnerJoin<T6>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> InnerJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> LeftJoin<T6>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> LeftJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> RightJoin<T6>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> RightJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> FullOuterJoin<T6>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> FullOuterJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> Where(Expression<Func<T, T2, T3, T4, T5, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> Having(Expression<Func<T, T2, T3, T4, T5, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> FullOuterJoin<T7>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> FullOuterJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> Where(Expression<Func<T, T2, T3, T4, T5, T6, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> Having(Expression<Func<T, T2, T3, T4, T5, T6, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> FullOuterJoin<T8>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> FullOuterJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> FullOuterJoin<T9>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> FullOuterJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> FullOuterJoin<T10>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> FullOuterJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FullOuterJoin<T11>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FullOuterJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FullOuterJoin<T12>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FullOuterJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FullOuterJoin<T13>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FullOuterJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FullOuterJoin<T14>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FullOuterJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FullOuterJoin<T15>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FullOuterJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FullOuterJoin<T16>();

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FullOuterJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> selectExpression);
    }

    public interface ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    {

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> whereExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> havingExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> groupByExpression);

        ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending);

        ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> selectExpression);
    }


    public class SqlFromDefinition<T> : BaseSqlFromDefinition, ISqlFromDefinition<T>
    {

        public ISqlBuilderExecutor SqlBuilderExecutor { get; set; }

        public SqlFromDefinition(ISqlBuilderExecutor sqlBuilderExecutor)
        {
            this.SqlBuilderExecutor = sqlBuilderExecutor;
        }

        public ISqlFromDefinition<T, T2> InnerJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2> InnerJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2> LeftJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2> LeftJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2> RightJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2> RightJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2> FullOuterJoin<T2>()
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2> FullOuterJoin<T2>(Expression<Func<T, T2, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T> Where(Expression<Func<T, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T> Having(Expression<Func<T, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T> GroupBy<TResult>(Expression<Func<T, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T> OrderBy<TResult>(Expression<Func<T, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2>
    {

        public SqlFromDefinition<T> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3> InnerJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3> InnerJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3> LeftJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3> LeftJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3> RightJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3> RightJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3> FullOuterJoin<T3>()
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3> FullOuterJoin<T3>(Expression<Func<T, T2, T3, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2> Where(Expression<Func<T, T2, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2> Having(Expression<Func<T, T2, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2> GroupBy<TResult>(Expression<Func<T, T2, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2> OrderBy<TResult>(Expression<Func<T, T2, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3>
    {

        public SqlFromDefinition<T, T2> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4> InnerJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4> InnerJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4> LeftJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4> LeftJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4> RightJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4> RightJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4> FullOuterJoin<T4>()
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4> FullOuterJoin<T4>(Expression<Func<T, T2, T3, T4, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3> Where(Expression<Func<T, T2, T3, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3> Having(Expression<Func<T, T2, T3, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3> GroupBy<TResult>(Expression<Func<T, T2, T3, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3> OrderBy<TResult>(Expression<Func<T, T2, T3, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4>
    {

        public SqlFromDefinition<T, T2, T3> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> InnerJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> LeftJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> RightJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> RightJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> FullOuterJoin<T5>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> FullOuterJoin<T5>(Expression<Func<T, T2, T3, T4, T5, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4> Where(Expression<Func<T, T2, T3, T4, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4> Having(Expression<Func<T, T2, T3, T4, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5>
    {

        public SqlFromDefinition<T, T2, T3, T4> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> InnerJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> InnerJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> LeftJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> LeftJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> RightJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> RightJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> FullOuterJoin<T6>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> FullOuterJoin<T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> Where(Expression<Func<T, T2, T3, T4, T5, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5> Having(Expression<Func<T, T2, T3, T4, T5, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> InnerJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> LeftJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> RightJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> FullOuterJoin<T7>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> FullOuterJoin<T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> Where(Expression<Func<T, T2, T3, T4, T5, T6, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> Having(Expression<Func<T, T2, T3, T4, T5, T6, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> InnerJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> LeftJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> RightJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> FullOuterJoin<T8>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> FullOuterJoin<T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> FullOuterJoin<T9>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> FullOuterJoin<T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> FullOuterJoin<T10>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> FullOuterJoin<T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FullOuterJoin<T11>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> FullOuterJoin<T11>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FullOuterJoin<T12>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> FullOuterJoin<T12>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FullOuterJoin<T13>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> FullOuterJoin<T13>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FullOuterJoin<T14>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> FullOuterJoin<T14>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FullOuterJoin<T15>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> FullOuterJoin<T15>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.InnerJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.InnerJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.LeftJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.LeftJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.RightJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.RightJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FullOuterJoin<T16>()
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.FullOuterJoin);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> FullOuterJoin<T16>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinExpression)
        {
            return new SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this, JoinType.FullOuterJoin, joinExpression);
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }

    public class SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : BaseSqlFromWithJoinDefinition, ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    {

        public SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> PreviousFromDefinition { get; set; }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> previousFromDefinition, JoinType joinType)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
        }

        public SqlFromDefinition(SqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> previousFromDefinition, JoinType joinType, Expression joinExpression)
        {
            this.PreviousFromDefinition = previousFromDefinition;
            this.JoinType = joinType;
            this.JoinExpression = joinExpression;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> whereExpression)
        {
            this.WhereExpressions.Add(whereExpression);
            return this;
        }
        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Having(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> havingExpression)
        {
            this.HavingExpressions.Add(havingExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> GroupBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> groupByExpression)
        {
            this.GroupByExpressions.Add(groupByExpression);
            return this;
        }

        public ISqlFromDefinition<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> orderByExpression, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            this.OrderByExpressions.Add(Tuple.Create((Expression)orderByExpression, sortDirection));
            return this;
        }

        public ISqlQuerySelection<TResult> Select<TResult>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>> selectExpression)
        {
            return new SqlQuerySelection<TResult>(this, selectExpression, this.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.PreviousFromDefinition.SqlBuilderExecutor);
        }
    }



}
