namespace TopHat.Engine {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;

    using Dapper;

    using TopHat.Configuration;
    using TopHat.Extensions;

    internal class WhereClauseWriter : IWhereClauseWriter {
        private ISqlDialect dialect;

        private IDictionary<Type, IMap> maps;

        public WhereClauseWriter(ISqlDialect dialect, IDictionary<Type, IMap> maps) {
            this.dialect = dialect;
            this.maps = maps;
        }

        public WhereClauseWriterResult GenerateSql<T>(IList<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode) {
            if (whereClauses.IsEmpty()) {
                return new WhereClauseWriterResult(string.Empty, null);
            }

            var sql = new StringBuilder(" where ");
            var parameters = new DynamicParameters();
            foreach (var whereClause in whereClauses) {
                var expressionVisitor = new WhereClauseExpressionVisitor(this.dialect, this.maps, rootNode);
                expressionVisitor.VisitTree(whereClause);
                sql.Append(expressionVisitor.Sql);
                sql.Append(" and ");
                parameters.AddDynamicParams(expressionVisitor.Parameters);
            }

            // remove the last and
            sql.Remove(sql.Length - 5, 5);
            return new WhereClauseWriterResult(sql.ToString(), parameters);
        }
    }
}