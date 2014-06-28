namespace TopHat.Engine.DDL {
    using System;
    using System.Text;

    using TopHat.Configuration;

    public class DropTableWriter : IDropTableWriter {
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

        public string DropTableIfExists(IMap map) {
            return this.dialect.WriteDropTableIfExists(map.Table);
        }
    }
}