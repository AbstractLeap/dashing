using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.SqlBuilder
{
    public interface ISqlQuerySelection<TResult> : IEnumerable<TResult>
    {
        Task<IEnumerable<TResult>> EnumerateAsync();
    }

    public interface ISqlBuilderExecutor
    {
        IEnumerable<T> Query<T>(BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression)
            where T : class, new();

        Task<IEnumerable<T>> QueryAsync<T>(BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression)
            where T : class, new();
    }

    public class SqlQuerySelection<TResult> : ISqlQuerySelection<TResult>
    {
        private readonly ISqlBuilderExecutor sqlBuilderExecutor;

        private readonly Expression selectExpression;

        private readonly BaseSqlFromDefinition fromDefinition;

        public SqlQuerySelection(BaseSqlFromDefinition fromDefinition, 
            Expression selectExpression,
            ISqlBuilderExecutor sqlBuilderExecutor)
        {
            this.selectExpression = selectExpression;
            this.sqlBuilderExecutor = sqlBuilderExecutor;
            this.fromDefinition = fromDefinition;
        }

        public IEnumerator<TResult> GetEnumerator()
        {
            return this.sqlBuilderExecutor.Query<TResult>(this.fromDefinition, this.selectExpression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync()
        {
            return this.sqlBuilderExecutor.QueryAsync<TResult>(this.fromDefinition, this.selectExpression);
        }
    }

    public abstract class BaseSqlFromDefinition
    {
        public IList<Expression> WhereExpressions { get; set; } = new List<Expression>();

        public IList<Expression> HavingExpressions { get; set; } = new List<Expression>();

        public IList<Expression> GroupByExpressions { get; set; } = new List<Expression>();

        public IList<Tuple<Expression, ListSortDirection>> OrderByExpressions { get; set; } = new List<Tuple<Expression, ListSortDirection>>();
    }

    public abstract class BaseSqlFromWithJoinDefinition : BaseSqlFromDefinition
    {
        public JoinType JoinType { get; set; }

        public Expression JoinExpression { get; set; }
    }

    public enum JoinType
    {
        InnerJoin,
        LeftJoin,
        RightJoin,
        FullOuterJoin
    }

    public class CommandDefinition
    {
        public string Sql { get; set; }

        public object Parameters { get; set; }
    }

    class SqlBuilder
    {
        private readonly ISqlBuilderExecutor sqlBuilderExecutor;

        public SqlBuilder(ISqlBuilderExecutor sqlBuilderExecutor)
        {
            this.sqlBuilderExecutor = sqlBuilderExecutor;
        }

        //public ISqlFromDefinition<T> From<T>()
        //{
        //    return new SqlFromDefinition<T>(this.session);
        //}
    }

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
