namespace Dashing.Engine.DML
{
    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML.Elements;
    using Dashing.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal sealed class WhereClauseWriter : WhereClauseWriterBase, IWhereClauseWriter
    {
        public WhereClauseWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config)
        {

        }

        public SelectWriterResult GenerateSql<T>(IEnumerable<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode)
        {
            if (whereClauses.IsEmpty())
            {
                return new SelectWriterResult(string.Empty, null, rootNode);
            }

            this.InitVariables();
            this.modifiedRootNode = rootNode;

            var first = true;
            foreach (var whereClause in whereClauses) {
                if (!first) {
                    this.ResetVariables();
                }

                this.VisitWhereClause(whereClause);
                this.sqlElements.Enqueue(new StringElement(" and "));
                first = false;
            }

            return new SelectWriterResult(this.GetSql(), this.parameters, this.modifiedRootNode);
        }
    }
}