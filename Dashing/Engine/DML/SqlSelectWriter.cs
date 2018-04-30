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
        private readonly MultiParameterWhereClausewriter whereClauseWriter;

        public SqlSelectWriter(ISqlDialect dialect, IConfiguration configuration)
        {
            this.dialect = dialect;
            this.configuration = configuration;
            this.whereClauseWriter = new MultiParameterWhereClausewriter(dialect, configuration);
        }

        public SqlWriterResult GenerateSql(BaseSqlFromDefinition sqlFromDefinition, Expression selectExpression)
        {
            var fromExpressions = new StringBuilder();
            var whereExpressions = new StringBuilder();
            var havingExpressions = new StringBuilder();
            var groupByExpressions = new StringBuilder();
            var orderByExpressions = new StringBuilder();

            this.VisitFromDefinition(sqlFromDefinition, fromExpressions, whereExpressions, groupByExpressions, havingExpressions, orderByExpressions);

            
            var sql = $@"
from {fromExpressions}
{(whereExpressions.Length > 0 ? "where " : "")}{whereExpressions}
{(groupByExpressions.Length > 0 ? "group by " : "")}{groupByExpressions}
{(havingExpressions.Length > 0 ? "where " : "")}{havingExpressions}
{(orderByExpressions.Length > 0 ? "order by " : "")}{orderByExpressions}

";

            return new SqlWriterResult(sql, null);
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
                var joinSql = this.whereClauseWriter.GenerateSql(sqlWithJoinExpression.JoinExpression);
                if (joinSql.Sql.Length > 0)
                {
                    fromExpressions.Append(" on ").Append(joinSql.Sql);
                    // TODO parameters
                }
            }
            else
            {
                // add in the base clause
                var baseType = sqlFromDefinition.GetType().GetGenericArguments().First();
                var map = this.configuration.GetMap(baseType);
                this.dialect.AppendQuotedTableName(fromExpressions, map);
                fromExpressions.Append(" as t1 ");
            }

            this.AppendLambdaExpressions(sqlFromDefinition.WhereExpressions, whereExpressions);
            this.AppendLambdaExpressions(sqlFromDefinition.HavingExpressions, havingExpressions);
        }

        private void AppendLambdaExpressions(IList<Expression> listOfExpressions, StringBuilder expressionStringBuilder)
        {
            if (listOfExpressions?.Any() ?? false)
            {
                if (expressionStringBuilder.Length > 0)
                {
                    expressionStringBuilder.Append(" and ");
                }
                foreach (var whereExpression in listOfExpressions)
                {
                    var sqlResult = this.whereClauseWriter.GenerateSql(whereExpression as LambdaExpression);
                    if (sqlResult.Sql.Length > 0)
                    {
                        expressionStringBuilder.Append(sqlResult.Sql);
                        // TODO parameters
                    }
                }
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

                case JoinType.CrossJoin:
                    stringBuilder.Append(" cross join ");
                    break;
            }
        }
    }
}