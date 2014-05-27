namespace TopHat.Engine.DDL {
    using System.Linq;
    using System.Text;

    using TopHat.Configuration;

    public class CreateTableWriter {
        private readonly ISqlDialect dialect;

        public CreateTableWriter(ISqlDialect dialect) {
            this.dialect = dialect;
        }

        public string CreateTable(IMap map) {
            var sql = new StringBuilder();

            sql.Append("create table ");
            this.dialect.AppendQuotedTableName(sql, map);
            sql.Append(" (");

            this.dialect.AppendColumnSpecification(sql, map.PrimaryKey);

            foreach (var column in map.Columns.Values.Where(c => !c.IsPrimaryKey && !c.IsIgnored)) {
                sql.Append(", ");
                this.dialect.AppendColumnSpecification(sql, column);
            }

            sql.Append(" )");
            return sql.ToString();
        }
    }
}