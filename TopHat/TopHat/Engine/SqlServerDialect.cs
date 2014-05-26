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

        protected override string TypeName(DbType type) {
            switch (type) {
                case DbType.Boolean:
                    return "bit";

                case DbType.DateTime2:
                    return "datetime";

                case DbType.Guid:
                    return "uniqueidentifier";

                case DbType.Object:
                    return "sql_variant";

                default:
                    return base.TypeName(type);
            }
        }
    }
}