namespace Dashing.SqlBuilder {
    public class SqlBuilder {
        private readonly ISqlBuilderExecutor sqlBuilderExecutor;

        public SqlBuilder(ISqlBuilderExecutor sqlBuilderExecutor) {
            this.sqlBuilderExecutor = sqlBuilderExecutor;
        }

        public ISqlFromDefinition<T> From<T>() {
            return new SqlFromDefinition<T>(this.sqlBuilderExecutor);
        }
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