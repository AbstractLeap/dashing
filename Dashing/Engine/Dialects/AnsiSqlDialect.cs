namespace Dashing.Engine.Dialects {
    using System;
    using System.Data;
    using System.Text;

    using Dashing.Configuration;

    public class AnsiSqlDialect : SqlDialectBase {
        protected override string TypeName(DbType type) {
            switch (type) {
                case DbType.Binary:
                    return "bit";

                case DbType.Boolean:
                    return "smallint unsigned";

                case DbType.Byte:
                    return "smallint unsigned";

                case DbType.DateTime:
                case DbType.DateTime2:
                    return "timestamp";

                case DbType.DateTimeOffset:
                    return "timestamptz";

                case DbType.Double:
                    return "double precision";

                default:
                    return base.TypeName(type);
            }
        }

        public override string ChangeColumnName(IColumn fromColumn, IColumn toColumn) {
            throw new InvalidOperationException("There is no Ansi-SQL way of changing a column name.");
        }

        public override string ModifyColumn(IColumn fromColumn, IColumn toColumn) {
            throw new InvalidOperationException("There is no Ansi-SQL way of changing a column type.");
        }

        public override void ApplySkipTake(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            throw new InvalidOperationException("There is no Ansi-SQL way of expressing an offset-limit / skip-take clause.");
        }
    }
}