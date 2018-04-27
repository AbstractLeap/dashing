namespace Dashing.SqlBuilder {
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface ISqlBuilderExecutor {
        IEnumerable<T> Query<T>(BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression);

        Task<IEnumerable<T>> QueryAsync<T>(BaseSqlFromDefinition baseSqlFromDefinition, Expression selectExpression);
    }
}