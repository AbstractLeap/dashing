namespace TopHat.Engine {
    using System.Data;
    using System.Text;

    using TopHat.Configuration;

    public class SqlServerDialect : SqlDialectBase {
        private const char DotCharacter = '.';

        public SqlServerDialect()
            : base('[', ']') { }

        public override void AppendQuotedTableName(StringBuilder sql, IMap map) {
            if (map.Schema != null) {
                this.AppendQuotedName(sql, map.Schema);
                sql.Append(DotCharacter);
            }

            this.AppendQuotedName(sql, map.Table);
        }

        protected override void AppendLength(StringBuilder sql, ushort length) {
            sql.Append("(");

            // TODO: this is a bit dodge, probs should think of a better way to do it
            if (length > 2000) {
                sql.Append("max");
            }
            else {
                sql.Append(length);
            }

            sql.Append(")");
        }

        protected override void AppendAutoGenerateModifier(StringBuilder sql) {
            sql.Append(" identity(1,1)");
        }

        protected override string TypeName(DbType type) {
            switch (type) {
                case DbType.Boolean:
                    return "bit";

                case DbType.DateTime2:
                    return "datetime2";

                case DbType.Guid:
                    return "uniqueidentifier";

                case DbType.Object:
                    return "sql_variant";

                default:
                    return base.TypeName(type);
            }
        }

        public override void ApplyPaging(StringBuilder sql, StringBuilder orderClause, int take, int skip) {
            if (take == 0) {
                return;
            }

            if (skip == 0) {
                // query starts with SELECT so insert top (X) there
                sql.Insert(6, " top (@take)");
                return;
            }

            // now we have take and skip - we'll do the recursive CTE thingy
            sql.Insert(6, " ROW_NUMBER() OVER (" + orderClause.ToString() + ") as RowNum,");
            sql.Insert(0, "select * from (");
            sql.Append(") as pagetable where pagetable.RowNum between @skip + 1 and @skip + @take order by pagetable.RowNum");
        }
    }
}