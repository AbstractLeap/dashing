namespace Dashing.Engine.DML {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML.Elements;
    using Dashing.Extensions;

    internal sealed class WhereClauseWriter : WhereClauseWriterBase, IWhereClauseWriter {
        public WhereClauseWriter(ISqlDialect dialect, IConfiguration config)
            : base(dialect, config) { }

        public SelectWriterResult GenerateSql<T>(IEnumerable<Expression<Func<T, bool>>> whereClauses, FetchNode rootNode) {
            if (whereClauses.IsEmpty()) {
                return new SelectWriterResult(string.Empty, null, rootNode);
            }

            this.InitVariables();
            this.modifiedRootNode = rootNode;

            foreach (var entry in whereClauses.AsSmartEnumerable()) {
                if (!entry.IsFirst) {
                    this.ResetVariables();
                }

                this.VisitWhereClause(entry.Value);
                if (!entry.IsLast) {
                    this.sqlElements.Enqueue(new StringElement(" and "));
                }
            }

            return new SelectWriterResult(this.GetSql(), this.parameters, this.modifiedRootNode);
        }
    }
}