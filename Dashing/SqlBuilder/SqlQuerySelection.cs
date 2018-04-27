namespace Dashing.SqlBuilder {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class SqlQuerySelection<TResult> : ISqlQuerySelection<TResult> {
        private readonly ISqlBuilderExecutor sqlBuilderExecutor;

        private readonly Expression selectExpression;

        private readonly BaseSqlFromDefinition fromDefinition;

        public SqlQuerySelection(BaseSqlFromDefinition fromDefinition, Expression selectExpression, ISqlBuilderExecutor sqlBuilderExecutor) {
            this.selectExpression = selectExpression;
            this.sqlBuilderExecutor = sqlBuilderExecutor;
            this.fromDefinition = fromDefinition;
        }

        public IEnumerator<TResult> GetEnumerator() {
            return this.sqlBuilderExecutor.Query<TResult>(this.fromDefinition, this.selectExpression)
                       .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        public Task<IEnumerable<TResult>> EnumerateAsync() {
            return this.sqlBuilderExecutor.QueryAsync<TResult>(this.fromDefinition, this.selectExpression);
        }
    }
}