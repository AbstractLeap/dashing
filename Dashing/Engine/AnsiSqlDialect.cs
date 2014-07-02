namespace Dashing.Engine {
    using System.Data;

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
    }
}