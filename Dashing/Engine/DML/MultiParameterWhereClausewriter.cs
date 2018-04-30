namespace Dashing.Engine.DML
{
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using System.Linq.Expressions;
    internal sealed class MultiParameterWhereClausewriter : WhereClauseWriterBase
    {
        public MultiParameterWhereClausewriter(ISqlDialect dialect, IConfiguration config) : base(dialect, config)
        {
            this.isMultiParameterWhereClauseWriter = true;
        }

        public SqlWriterResult GenerateSql(LambdaExpression whereClause)
        {
            if (whereClause == null)
            {
                return new SqlWriterResult(string.Empty, null);
            }

            this.InitVariables();
            this.VisitWhereClause(whereClause);
            return new SqlWriterResult(this.GetSql(), this.parameters);
        }
    }
}