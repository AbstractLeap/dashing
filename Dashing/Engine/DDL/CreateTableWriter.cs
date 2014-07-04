namespace Dashing.Engine.DDL {
    using System;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class CreateTableWriter : ICreateTableWriter {
        private readonly ISqlDialect dialect;

        public CreateTableWriter(ISqlDialect dialect) {
            if (dialect == null) {
                throw new ArgumentNullException("dialect");
            }

            this.dialect = dialect;
        }

        public string CreateTable(IMap map) {
            var sql = new StringBuilder();

            sql.Append("create table ");
            this.dialect.AppendQuotedTableName(sql, map);
            sql.Append(" (");

            this.dialect.AppendColumnSpecification(sql, map.PrimaryKey);

            foreach (var column in map.OwnedColumns(true).Where(c => !c.IsPrimaryKey)) {
                sql.Append(", ");
                this.dialect.AppendColumnSpecification(sql, column);
            }

            sql.Append(")");
            return sql.ToString();
        }
    }
}