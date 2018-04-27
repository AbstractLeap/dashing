namespace Dashing.Engine.DML
{

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.SqlBuilder;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    internal class SqlSelectWriter
    {
        private readonly ISqlDialect dialect;
        private readonly IConfiguration configuration;

        public SqlSelectWriter(ISqlDialect dialect, IConfiguration configuration)
        {
            this.dialect = dialect;
            this.configuration = configuration;
        }

        public SqlWriterResult GenerateSql(BaseSqlFromDefinition sqlFromDefinition, Expression selectExpression)
        {
            var fromExpressions = new StringBuilder();
            var whereExpressions = new StringBuilder();
            var havingExpressions = new StringBuilder();
            var groupByExpressions = new StringBuilder();
            var orderByExpressions = new StringBuilder();

            this.VisitFromDefinition(sqlFromDefinition, fromExpressions, whereExpressions, groupByExpressions, havingExpressions, orderByExpressions);

            return new SqlWriterResult(fromExpressions.ToString(), null);
        }

        private void VisitFromDefinition(BaseSqlFromDefinition sqlFromDefinition, StringBuilder fromExpressions, StringBuilder whereExpressions, StringBuilder groupByExpressions, StringBuilder havingExpressions, StringBuilder orderByExpressions)
        {
            if (sqlFromDefinition is BaseSqlFromWithJoinDefinition sqlWithJoinExpression)
            {
                this.VisitFromDefinition(sqlWithJoinExpression.PreviousFromDefinition, fromExpressions, whereExpressions, groupByExpressions, havingExpressions, orderByExpressions);

                // now add in the clauses for this definition
                var fromDefinitionTypes = sqlWithJoinExpression.GetType().GetGenericArguments();
                var joinedType = fromDefinitionTypes.Last();
                var map = this.configuration.GetMap(joinedType);
                this.AppendJoin(fromExpressions, sqlWithJoinExpression.JoinType);
                this.dialect.AppendQuotedTableName(fromExpressions, map);
                fromExpressions.Append(" as t").Append(fromDefinitionTypes.Length).Append(" ");
            } else
            {
                // add in the base clause
                var baseType = sqlFromDefinition.GetType().GetGenericArguments().First();
                var map = this.configuration.GetMap(baseType);
                this.dialect.AppendQuotedTableName(fromExpressions, map);
                fromExpressions.Append(" as t1 ");
            }
        }
        
        private void AppendJoin(StringBuilder stringBuilder, JoinType joinType)
        {
            switch(joinType)
            {
                case JoinType.InnerJoin:
                    stringBuilder.Append(" inner join ");
                    break;

                case JoinType.LeftJoin:
                    stringBuilder.Append(" left join ");
                    break;

                case JoinType.RightJoin:
                    stringBuilder.Append(" right join ");
                    break;

                case JoinType.FullOuterJoin:
                    stringBuilder.Append(" full outer join ");
                    break;
            }
        }
    }
}