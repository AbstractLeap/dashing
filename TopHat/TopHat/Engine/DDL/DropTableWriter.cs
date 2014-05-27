namespace TopHat.Engine.DDL {
    using System;
    using System.Text;

    using TopHat.Configuration;

    public class DropTableWriter {
        private readonly ISqlDialect dialect;

        public DropTableWriter(ISqlDialect dialect) {
            if (dialect == null) {
                throw new ArgumentNullException("dialect");
            }

            this.dialect = dialect;
        }

        public string DropTable(IMap map) {
            var sql = new StringBuilder();
            sql.Append("drop table ");
            this.dialect.AppendQuotedTableName(sql, map);
            return sql.ToString();
        }
    }
}